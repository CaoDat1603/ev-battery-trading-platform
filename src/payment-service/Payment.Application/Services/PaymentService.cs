using System;
using System.Threading.Tasks;
using Payment.Application.Contracts;
using Payment.Application.DTOs;
using Payment.Domain.Abstraction;
using Payment.Domain.Enums;

namespace Payment.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IOrderServiceClient _orderServiceClient;

        // Value taken from Order.Domain.Enums.TransactionStatus.Processing (2)
        private const int OrderTransactionProcessingStatus = 2;

        public PaymentService(IPaymentRepository paymentRepository, IVnPayService vnPayService, IOrderServiceClient orderServiceClient)
        {
            _paymentRepository = paymentRepository;
            _vnPayService = vnPayService;
            _orderServiceClient = orderServiceClient;
        }

        public async Task<string> CreatePaymentUrl(CreatePaymentRequest request, string ipAddress)
        {
            //1. Hỏi Order Service số tiền phải thu từ người mua
            var transactionDetails = await _orderServiceClient.GetTransactionByIdAsync(request.TransactionId);

            //2. Kiểm tra nghiệp vụ
            if (transactionDetails == null)
                throw new Exception($"Transaction with ID {request.TransactionId} not found.");

            if (transactionDetails.BuyerAmount <= 0)
                throw new InvalidOperationException("Invalid transaction amount. Amount must be positive.");

            decimal amount = transactionDetails.BuyerAmount;

            //3. Tạo Entity Payment và lưu DB (status: pending) với amount = buyerAmount
            var payment = new Domain.Entities.Payment(request.TransactionId, "VNPay", amount);
            await _paymentRepository.AddAsync(payment);

            //3. Gọi VNPAY Service để tạo URL
            return _vnPayService.CreatePaymentUrl(payment.PaymentId, amount, ipAddress);
        }

        public async Task<bool> HandleVnPayReturn(string queryString)
        {
            //1. Xác thực chữ ký
            if (!_vnPayService.ValidateSignature(queryString)) return false;

            //2. Lấy dữ liệu VNPAY
            var responseData = _vnPayService.GetResponseData(queryString);
            var vnpResponseCode = responseData["vnp_ResponseCode"];
            var paymentId = int.Parse(responseData["vnp_TxnRef"]);
            var vnpTransactionNo = responseData["vnp_TransactionNo"]; // Mã giao dịch của VNPAY

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) return false;

            //3. Xử lý trạng thái dựa trên mã phản hồi VNPAY (00: thành công, khác00: không thành công)
            if (responseData["vnp_ResponseCode"] == "00")
            {
                payment.MarkAsSuccess(responseData["vnp_TransactionNo"]);
                await _paymentRepository.UpdateAsync(payment);

                // Gọi đến Order Service sau khi thanh toán thành công
                // Cập nhật trạng thái Transaction thành 'Processing'
                bool updateSuccess = await _orderServiceClient.UpdateTransactionStatusAsync(payment.TransactionId, OrderTransactionProcessingStatus);
                if (!updateSuccess)
                {
                    // Xử lý lỗi: Cần Message Queue (RabbitMQ/Kafka) để retry sau
                    Console.WriteLine($"WARNING: Failed to update Order status for Transaction ID: {payment.TransactionId}");
                }
            }
            else
            {
                payment.MarkAsFailed();
            }

            //4. Cập nhật Payment Entity
            await _paymentRepository.UpdateAsync(payment);

            return vnpResponseCode == "00";
        }

        public async Task<bool> InitiateRefund(int transactionId)
        {
            // 1. Tìm bản ghi Payment liên quan
            var payment = await _paymentRepository.GetSuccessfulPaymentByTransactionIdAsync(transactionId);

            // 2. Kiểm tra điều kiện hoàn tiền
            if (payment == null)
            {
                // Không thể hoàn tiền nếu không tìm thấy Payment
                Console.WriteLine($"Error: No success payment found for Transaction ID {transactionId}.");
                return false;
            }
            if (payment.Status != PaymentStatus.Success)
            {
                // Không thể hoàn tiền nếu trạng thái giao dịch khác thành công
                Console.WriteLine($"Error: Payment status is {payment.Status}, not eligible for refund.");
                return false;
            }

            // --- 3. Gọi API hoàn tiền VNPay ---
            string vnpayResponseCode = await _vnPayService.RequestVnPayRefundAsync(payment.PaymentId, payment.Amount);

            bool refundSuccess = vnpayResponseCode == "00";

            if (refundSuccess)
            {
                payment.MarkAsRefunded(); // Cập nhật trạng thái Entity
                await _paymentRepository.UpdateAsync(payment);

                // --- 4. Gửi thông báo cho Order Service ---
                await _orderServiceClient.NotifyRefundCompletionAsync(transactionId, true);

                return true;
            }
            // Hoàn tiền thất bại: Gửi thông báo thất bại
            await _orderServiceClient.NotifyRefundCompletionAsync(transactionId, false);
            return false;
        }

        public async Task<(bool ok, string rspCode, string message)> HandleVnPayIpnAsync(string queryString)
        {
            // 1) Validate chữ ký
            if (!_vnPayService.ValidateSignature(queryString))
                return (false, "97", "Invalid signature"); // theo VNPay: 97 = checksum sai

            // 2) Parse
            var data = _vnPayService.GetResponseData(queryString);
            if (!data.TryGetValue("vnp_TxnRef", out var txnRef) || !int.TryParse(txnRef, out var paymentId))
                return (false, "01", "Order not found"); // 01 = không tìm thấy đơn

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) return (false, "01", "Order not found");

            // 3) (Khuyến nghị) Xác thực amount
            if (!data.TryGetValue("vnp_Amount", out var rawAmount)) return (false, "04", "Invalid amount");
            // VNPay *100*
            var amountFromVnp = decimal.Parse(rawAmount) / 100m;
            if (amountFromVnp != payment.Amount)
                return (false, "04", "Invalid amount"); // 04 = dữ liệu không hợp lệ

            // 4) Cập nhật trạng thái theo ResponseCode
            var code = data.TryGetValue("vnp_ResponseCode", out var rc) ? rc : "99";
            if (code == "00")
            {
                var vnpTxnNo = data.TryGetValue("vnp_TransactionNo", out var vnptx) ? vnptx : "";
                payment.MarkAsSuccess(vnpTxnNo);
                await _paymentRepository.UpdateAsync(payment);

                // Đẩy Order sang trạng thái Processing
                var upd = await _orderServiceClient.UpdateTransactionStatusAsync(payment.TransactionId, 2);
                if (!upd) Console.WriteLine($"WARN: Update Order status fail for Tx:{payment.TransactionId}");

                return (true, "00", "Confirm Success");
            }
            else
            {
                payment.MarkAsFailed();
                await _paymentRepository.UpdateAsync(payment);
                return (false, "00", "Confirm Success"); // vẫn trả 00 để VNPay không gọi lại nữa
            }
        }

        // Các phương thức GET (Mapping) ---
        public async Task<IEnumerable<PaymentDto>> GetPaymentsByTransactionIdAsync(int transactionId)
        {
            var payments = await _paymentRepository.GetPaymentsByTransactionIdAsync(transactionId);
            return payments.Select(MapToDto);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(MapToDto);
        }

        private PaymentDto MapToDto(Domain.Entities.Payment p)
        {
            return new PaymentDto
            {
                PaymentId = p.PaymentId,
                TransactionId = p.TransactionId,
                Method = p.Method,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                ReferenceCode = p.ReferenceCode ?? string.Empty,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
