using Payment.Application.Contracts;
using Payment.Application.DTOs;
using Payment.Domain.Abstraction;
using Payment.Domain.Enums;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

namespace Payment.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IOrderServiceClient _orderServiceClient;

        // Value taken from Order.Domain.Enums.TransactionStatus.Processing (2)
        private const int OrderTransactionProcessingStatus = 2;
        // TransactionStatus.Completed (3)
        private const int OrderTransactionCompletedStatus = 3;

        public PaymentService(IPaymentRepository paymentRepository, IVnPayService vnPayService, IOrderServiceClient orderServiceClient)
        {
            _paymentRepository = paymentRepository;
            _vnPayService = vnPayService;
            _orderServiceClient = orderServiceClient;
        }

        public async Task<string> CreatePaymentUrl(CreatePaymentRequest request, string ipAddress)
        {
            //1. Lấy transaction từ Order Service
            var transaction = await _orderServiceClient.GetTransactionByIdAsync(request.TransactionId);
            if (transaction == null)
            {
                throw new ArgumentException($"Transaction with ID {request.TransactionId} not found in Order Service.");
            }

            // 2. Không tìm thấy transaction
            if (transaction == null)
            {
                throw new BusinessException("TRANSACTION_NOT_FOUND", $"Transaction with ID {request.TransactionId} does not exist.", 404);
            }

            // 3. Số tiền không hợp lệ
            if (transaction.BuyerAmount <= 0)
            {
                throw new BusinessException("INVALID_AMOUNT", $"BuyerAmount must be greater than 0 (got {transaction.BuyerAmount}).", 400);
            }

            // 4. Trạng thái không hợp lệ (chỉ cho phép Pending)
            if (!string.Equals(transaction.TransactionStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("INVALID_TRANSACTION_STATUS", $"Transaction {request.TransactionId} is not in Pending status (current: {transaction.TransactionStatus}).", 400);
            }

            // Variable to capture vnp_CreateDate from VNPay
            string vnPayCreateDate;

            // 5. Kiểm tra payment đã tồn tại chưa (đã thanh toán thành công)
            var successPayment = await _paymentRepository.GetSuccessfulPaymentByTransactionIdAsync(request.TransactionId);
            if (successPayment != null)
            {
                throw new BusinessException("PAYMENT_ALREADY_SUCCESS", $"Transaction with ID {request.TransactionId} already has a completed payment.", 409);
            }

            // 6. Tìm payment đang chờ (tái sử dụng nếu có)
            var pendingPayment = await _paymentRepository.GetPendingPaymentByTransactionIdAsync(request.TransactionId);
            if (pendingPayment != null)
            {
                Console.WriteLine($"[PaymentService] Reusing existing PENDING payment. TransactionId={request.TransactionId}, PaymentId={pendingPayment.PaymentId}, Amount={pendingPayment.Amount}");
                return _vnPayService.CreatePaymentUrl(pendingPayment.PaymentId, pendingPayment.Amount, ipAddress, out vnPayCreateDate);
            }

            // 7. Tạo Payment mới
            var payment = new Domain.Entities.Payment(request.TransactionId, "VNPay", transaction.BuyerAmount);
            await _paymentRepository.AddAsync(payment);

            // 8. Tạo URL thanh toán + lấy vnp_CreateDate gốc
            var paymentUrl = _vnPayService.CreatePaymentUrl(payment.PaymentId, payment.Amount, ipAddress, out vnPayCreateDate);

            // 9. Lưu vnp_CreateDate vào DB
            var txnRef = $"{payment.PaymentId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            payment.SetVnPayCreateDate(vnPayCreateDate);
            payment.SetVnPayTxnRef(txnRef);
            await _paymentRepository.UpdateAsync(payment);
            return paymentUrl;
        }

        public async Task<(bool Success, int? TransactionId)> HandleVnPayReturn(string queryString)
        {
            // 1.Xác thực chữ ký VNPay
            if (!_vnPayService.ValidateSignature(queryString))
            {
                return (false, null);
            }

            // 2. Lấy dữ liệu VNPay
            var responseData = _vnPayService.GetResponseData(queryString);
            var vnpResponseCode = responseData["vnp_ResponseCode"];
            var paymentId = int.Parse(responseData["vnp_TxnRef"]);
            var vnpTransactionNo = responseData["vnp_TransactionNo"];

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return (false, null);
            }

            // 3. Update payment + transaction status
            if (vnpResponseCode == "00")
            {
                payment.MarkAsSuccess(vnpTransactionNo);
                await _paymentRepository.UpdateAsync(payment);

                var updateOk = await _orderServiceClient.UpdateTransactionStatusAsync(
                    payment.TransactionId,
                    OrderTransactionCompletedStatus // Set to Completed (3)
                );

                if (!updateOk)
                {
                    Console.WriteLine($"WARNING: Failed to update Order status for Transaction ID: {payment.TransactionId}");
                }
            }
            else
            {
                payment.MarkAsFailed();
                await _paymentRepository.UpdateAsync(payment);
            }

            var ok = vnpResponseCode == "00";
            return (ok, payment.TransactionId);
        }

        public async Task<VnPayIpnResponse> HandleVnPayIpnAsync(string rawQuery)
        {
            //1. Check checksum
            var isValidSignature = _vnPayService.ValidateSignature(rawQuery);
            if (!isValidSignature)
            {
                // 97: Sai checksum -> VNPAY sẽ retry
                return new VnPayIpnResponse("97", "Invalid signature");
            }

            var data = HttpUtility.ParseQueryString(rawQuery);

            var vnpTmnCode = data["vnp_TmnCode"];
            var vnpAmountRaw = data["vnp_Amount"];
            var vnpBankCode = data["vnp_BankCode"];
            var vnpPayDate = data["vnp_PayDate"];
            var vnpOrderInfo = data["vnp_OrderInfo"];
            var vnpTransactionNo = data["vnp_TransactionNo"];
            var vnpResponseCode = data["vnp_ResponseCode"];
            var vnpTransactionStatus = data["vnp_TransactionStatus"];
            var vnpTxnRef = data["vnp_TxnRef"];


            if (string.IsNullOrEmpty(vnpTxnRef) || !int.TryParse(vnpTxnRef, out var paymentId))
            {
                // 01: Order không tồn tại / sai định dạng
                return new VnPayIpnResponse("01", "Order not found");
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return new VnPayIpnResponse("01", "Order not found");
            }

            //2. So sánh số tiền
            if (!long.TryParse(vnpAmountRaw, out var vnpAmount100))
            {
                return new VnPayIpnResponse("04", "Invalid amount");
            }

            var vnpAmount = vnpAmount100 / 100m; // VNPAY gửi *100
            if (payment.Amount != vnpAmount)
            {
                // 04: Số tiền không khớp -> VNPAY sẽ retry
                return new VnPayIpnResponse("04", "Amount mismatch");
            }

            //3. Đã xử lý rồi thì trả '02'
            if (payment.Status == PaymentStatus.Success || payment.Status == PaymentStatus.Failed)
            {
                return new VnPayIpnResponse("02", "Order already confirmed");
            }

            //4. Cập nhật trạng thái theo VNPay
            if (vnpResponseCode == "00" && vnpTransactionStatus == "00")
            {
                payment.MarkAsSuccess(vnpTransactionNo ?? string.Empty, vnpPayDate);
                await _paymentRepository.UpdateAsync(payment);

                // Gọi Order Service: set TransactionStatus = Completed (3)
                var updated = await _orderServiceClient.UpdateTransactionStatusAsync(payment.TransactionId, OrderTransactionCompletedStatus);

                if (!updated)
                {
                    Console.WriteLine($"[PAYMENT][IPN] WARNING: Failed to update Order status for Transaction {payment.TransactionId}");
                    // TODO: Có thể push vào queue để retry ở đây
                }
            }
            else
            {
                payment.MarkAsFailed(vnpTransactionNo ?? string.Empty);
                await _paymentRepository.UpdateAsync(payment);
            }

            //5. Merchant xử lý xong => Luôn trả về 00 để VNPAY dừng retry
            return new VnPayIpnResponse("00", "Confirm Success");
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

    public class BusinessException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public BusinessException(string code, string message, int statusCode = 400)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}
