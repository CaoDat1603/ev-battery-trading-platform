namespace Payment.Application.Contracts
{
    public class OrderTransactionDto
    {
        public int TransactionId { get; set; }
        public decimal BuyerAmount { get; set; }
        public int ProductId { get; set; }
        public string TransactionStatus { get; set; } = string.Empty;
    }

    public interface IOrderServiceClient
    {
        // Phương thức gọi Order API để cập nhật Transaction Status
        Task<bool> UpdateTransactionStatusAsync(int transactionId, int status, CancellationToken ct = default);

        // Thông báo kết quả hoàn tiền tới Order Service
        Task<bool> NotifyRefundCompletionAsync(int transactionId, bool isSuccess, CancellationToken ct = default);
        Task<OrderTransactionDto?> GetTransactionByIdAsync(int transactionId, CancellationToken ct = default);
    }
}
