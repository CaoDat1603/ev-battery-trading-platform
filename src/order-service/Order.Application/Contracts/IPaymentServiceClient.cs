namespace Order.Application.Contracts
{
    public interface IPaymentServiceClient
    {
        Task<bool> UpdateTransactionStatusAsync(int transactionId, int status);

        // Gửi yêu cầu hoàn tiền cho một giao dịch đã được thanh toán
        Task<bool> RequestRefundAsync(int transactionId);
    }
}
