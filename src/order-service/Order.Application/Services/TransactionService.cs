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
        private readonly IFeeSettingsRepository _feeSettingsRepository;
        private readonly IEventBus _eventBus;

        public TransactionService(ITransactionRepository transactionRepository, IFeeSettingsRepository feeSettingsRepository, IPaymentServiceClient paymentServiceClient, IEventBus eventBus)
        {
            _transactionRepository = transactionRepository;
            _paymentServiceClient = paymentServiceClient;
            _feeSettingsRepository = feeSettingsRepository;
            _eventBus = eventBus;
        }

        public async Task<int> CreateNewTransaction(CreateTransactionRequest request, int buyerId)
        {
            var feeSettings = await _feeSettingsRepository.GetActiveFeeSettingsAsync(request.ProductType);
            if (feeSettings == null) throw new InvalidOperationException($"Fee settings for product type {request.ProductType} not found.");

            var basePrice = request.BasePrice;
            var commissionFee = basePrice * (feeSettings.CommissionPercent / 100);
            var serviceFee = basePrice * (feeSettings.FeePercent / 100);
            var platformAmount = commissionFee + serviceFee;
            var buyerAmount = basePrice + serviceFee;
            var sellerAmount = basePrice - commissionFee;

            var transaction = new Transaction(
                request.ProductId,
                request.SellerId,
                //request.BuyerId,
                buyerId,
                feeSettings.FeeId,
                request.ProductType,
                basePrice,
                commissionFee,
                serviceFee,
                buyerAmount,
                sellerAmount,
                platformAmount
            );

            await _transactionRepository.AddAsync(transaction);

            var noti1 = new OrderNotificationEvent(buyerId, transaction.TransactionId, "Giao dịch đã được khởi tạo.", "Khởi tạo thành công.");
            await _eventBus.PublishAsync("order_exchange", noti1);
            return transaction.TransactionId;
        }

        public async Task<bool> UpdateTransactionStatus(int transactionId, TransactionStatus newStatus)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null) return false;

            try
            {
                transaction.UpdateStatus(newStatus);
                await _transactionRepository.UpdateAsync(transaction);
                var noti1 = new OrderNotificationEvent(transaction.BuyerId, transaction.TransactionId, "Giao dịch đã được cập nhập.", "Cập nhập thành công.");
                await _eventBus.PublishAsync("order_exchage", noti1);
                var noti2 = new OrderNotificationEvent(transaction.SellerId, transaction.TransactionId, "Giao dịch đã được cập nhập.", "Cập nhập thành công.");
                await _eventBus.PublishAsync("order_exchage", noti2);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public async Task<bool> CancelTransaction(int transactionId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null) return false;

            var wasProcessing = transaction.TransactionStatus == TransactionStatus.Processing;

            try
            {
                transaction.Cancel();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            if (wasProcessing)
            {
                var refundRequestSuccess = await _paymentServiceClient.RequestRefundAsync(transactionId);
                if (!refundRequestSuccess)
                {
                    Console.WriteLine($"CRITICAL FAILURE: Refund request failed for Transaction {transactionId}. Order state is Cancelled, but refund is pending.");
                }
            }

            await _transactionRepository.UpdateAsync(transaction);
            var noti1 = new OrderNotificationEvent(transaction.BuyerId, transaction.TransactionId, "Giao dịch đã được hủy.", "Giao dịch của bạn đã hủy.");
            await _eventBus.PublishAsync("order_exchage", noti1);
            return true;
        }

        public async Task<bool> HandleRefundNotificationAsync(int transactionId, bool refundSuccess)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null) return false;

            if (refundSuccess)
            {
                transaction.UpdateStatus(TransactionStatus.Cancelled);
                await _transactionRepository.UpdateAsync(transaction);
                return true;
            }

            Console.WriteLine($"WARNING: Refund failed notification for Transaction {transactionId}.");
            return false;
        }

        public async Task<TransactionDto?> GetTransactionByIdAsync(int transactionId, int loggedInUserId, bool isAdmin)
        {
            var tx = await _transactionRepository.GetByIdAsync(transactionId);
            if (tx == null) return null;
            if (!isAdmin && tx.BuyerId != loggedInUserId && tx.SellerId != loggedInUserId) return null;
            return MapToDto(tx);
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByBuyerAsync(int buyerId)
        {
            var txs = await _transactionRepository.GetByBuyerIdAsync(buyerId);
            return txs.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsBySellerAsync(int sellerId)
        {
            var txs = await _transactionRepository.GetBySellerIdAsync(sellerId);
            return txs.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
        {
            var txs = await _transactionRepository.GetAllAsync();
            return txs.Select(MapToDto);
        }
        public async Task<TransactionDto?> GetTransactionByIdForInternalAsync(int transactionId)
        {
            var tx = await _transactionRepository.GetByIdAsync(transactionId);
            return MapToDto(tx);
        }

        private static TransactionDto MapToDto(Transaction tx)
        {
            return new TransactionDto
            {
                TransactionId = tx.TransactionId,
                ProductId = tx.ProductId,
                SellerId = tx.SellerId,
                BuyerId = tx.BuyerId,
                ProductType = tx.ProductType,
                TransactionStatus = tx.TransactionStatus.ToString(),
                BasePrice = tx.BasePrice,
                BuyerAmount = tx.BuyerAmount,
                SellerAmount = tx.SellerAmount,
                PlatformAmount = tx.PlatformAmount,
                CreatedAt = tx.CreatedAt,
                UpdatedAt = tx.UpdatedAt,
                DeletedAt = tx.DeletedAt
            };
        }
    }
    public record OrderNotificationEvent(int userId, int transactionId, string title, string content);
}