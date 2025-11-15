namespace Payment.Domain.Abstraction
{
    public interface IPaymentRepository
    {
        Task<Payment.Domain.Entities.Payment?> GetByIdAsync(int id);
        Task AddAsync(Payment.Domain.Entities.Payment payment);
        Task UpdateAsync(Payment.Domain.Entities.Payment payment);

        // Tìm bản ghi Payment thành công gần nhất dựa trên TransactionId
        Task<Payment.Domain.Entities.Payment?> GetSuccessfulPaymentByTransactionIdAsync(int transactionId);
        Task<IEnumerable<Payment.Domain.Entities.Payment>> GetPaymentsByTransactionIdAsync(int transactionId);
        Task<IEnumerable<Payment.Domain.Entities.Payment>> GetAllAsync(); // Cho Admin
        Task<Payment.Domain.Entities.Payment?> GetPendingPaymentByTransactionIdAsync(int transactionId);
    }
}
