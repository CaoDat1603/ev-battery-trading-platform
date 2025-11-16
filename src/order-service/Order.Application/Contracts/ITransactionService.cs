using Order.Application.DTOs;
using Order.Domain.Enums;

namespace Order.Application.Contracts
{
    public interface ITransactionService
    {
        Task<int> CreateNewTransaction(CreateTransactionRequest request, int buyerId);
        Task<TransactionDto?> GetTransactionByIdAsync(int transactionId, int loggedInUserId, bool isAdmin);
        Task<IEnumerable<TransactionDto>> GetTransactionsByBuyerAsync(int buyerId);
        Task<IEnumerable<TransactionDto>> GetTransactionsBySellerAsync(int sellerId);
        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(); // Admin
        Task<bool> UpdateTransactionStatus(int transactionId, TransactionStatus newStatus);
        Task<bool> CancelTransaction(int transactionId);
        Task<bool> HandleRefundNotificationAsync(int transactionId, bool refundSuccess);
        Task<TransactionDto?> GetTransactionByIdForInternalAsync(int transactionId);
    }
}