namespace Payment.Domain.Abstraction
{
    public interface IPaymentRepository
    {
        Task<Payment.Domain.Entities.Payment?> GetByIdAsync(int id);
        Task AddAsync(Payment.Domain.Entities.Payment payment);
        Task UpdateAsync(Payment.Domain.Entities.Payment payment);
    }
}
