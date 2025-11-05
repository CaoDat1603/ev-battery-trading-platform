using Order.Application.Contracts;
using Order.Application.DTOs;
using Order.Domain.Abstraction;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentServiceClient _paymentServiceClient;
        
        // Dependency Injection qua constructor
        public TransactionService(ITransactionRepository transactionRepository, IPaymentServiceClient paymentServiceClient)
        {
            _transactionRepository = transactionRepository;
            _paymentServiceClient = paymentServiceClient;
        }

        public async Task<int> CreateNewTransaction(CreateTransactionRequest request)
        {
            // Sử dụng constructor nghiệp vụ để tạo đối tượng
            var transaction = new Transaction(
                request.ProductId,
                request.SellerId,
                request.BuyerId,
                request.FeeId,
                request.ProductType,
                request.BasePrice,
                request.CommissionFee,
                request.ServiceFee,
                request.BuyerAmount,
                request.SellerAmount,
                request.PlatformAmount
            );

            await _transactionRepository.AddAsync(transaction);
            return transaction.TransactionId;
        }

        public async Task<bool> UpdateTransactionStatus(int transactionId, TransactionStatus newStatus)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null) return false;

            try
            {
                // Gọi phương thức nghiệp vụ của Entity
                transaction.UpdateStatus(newStatus);
                await _transactionRepository.UpdateAsync(transaction);
                return true;
            }
            catch (InvalidOperationException)
            {
                // Lỗi nghiệp vụ (ví dụ: cố gắng chuyển trạng thái không hợp lệ)
                return false;
            }
        }

        public async Task<bool> CancelTransaction(int transactionId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null) return false;

            // 1. Kiểm tra trạng thái trước khi hủy
            bool wasProcessing = transaction.TransactionStatus == TransactionStatus.Processing;

            try
            {
                transaction.Cancel(); // Entity ghi nhận ý định hủy
            }
            catch (InvalidOperationException)
            {
                // Lỗi: ví dụ, cố gắng hủy một giao dịch đã hoàn thành
                return false;
            }

            // 2. Nếu đã thanh toán (Processing), YÊU CẦU HOÀN TIÈN
            if (wasProcessing)
            {
                Console.WriteLine($"Initiating refund for Transaction ID: {transactionId}");
                bool refundRequestSuccess = await _paymentServiceClient.RequestRefundAsync(transactionId);

                if (!refundRequestSuccess)
                {
                    // Lỗi: Payment Service không chấp nhận yêu cầu hoàn tiền (hoặc lỗi kết nối)
                    // HỆ THỐNG CẦN CÓ CƠ CHẾ SAGA/RETRY (Ví dụ: lưu vào Outbox/Message Queue)
                    Console.WriteLine($"CRITICAL FAILURE: Refund request failed for Transaction {transactionId}. Order state is Cancelled, but refund is pending.");
                    // Vẫn lưu trạng thái hủy vào Order DB để tránh việc người dùng gửi yêu cầu hủy lần nữa.
                    // Tuy nhiên, việc xử lý retry phải nằm ngoài phạm vi này.
                }
            }

            // 3. Cập nhật trạng thái hủy vào DB Order
            await _transactionRepository.UpdateAsync(transaction);
            return true;
        }
    }
}