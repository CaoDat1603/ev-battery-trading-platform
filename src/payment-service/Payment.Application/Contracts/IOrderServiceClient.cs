namespace Payment.Application.Contracts
{
    public class OrderTransactionDto
    {
        public int TransactionId { get; set; }
        public decimal BuyerAmount { get; set; }
        public int ProductId { get; set; }
    }

    public interface IOrderServiceClient
    {
        // Phương thức gọi Order API để cập nhật Transaction Status
        Task<bool> UpdateTransactionStatusAsync(int transactionId, int status);

        // Thông báo kết quả hoàn tiền tới Order Service
        Task<bool> NotifyRefundCompletionAsync(int transactionId, bool isSuccess);
        Task<OrderTransactionDto?> GetTransactionByIdAsync(int transactionId);
    }
}
