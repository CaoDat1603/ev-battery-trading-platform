using Microsoft.EntityFrameworkCore;
using Payment.Domain.Abstraction;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _dbContext;

        // DI cho DbContext
        public PaymentRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Payment.Domain.Entities.Payment?> GetByIdAsync(int id)
        {
            // Tìm theo khoá chính
            return await _dbContext.Payments.FindAsync(id);
        }

        public async Task AddAsync(Payment.Domain.Entities.Payment payment)
        {
            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment.Domain.Entities.Payment payment)
        {
            // EF Core tự động theo dõi và cập nhật khi SaveChangesAsync được gọi
            _dbContext.Payments.Update(payment);
            await _dbContext.SaveChangesAsync();
        }

        // Tìm bản ghi Payment thành công gần nhất dựa trên TransactionId
        public async Task<Payment.Domain.Entities.Payment?> GetSuccessfulPaymentByTransactionIdAsync(int transactionId)
        {
            // Lấy giao dịch thanh toán THÀNH CÔNG (Status = Success) gần nhất
            // Đây là bản ghi cần hoàn tiền
            return await _dbContext.Payments
                //.Where(p => p.TransactionId == transactionId && p.Status == Payment.Domain.Enums.PaymentStatus.Success)
                .OrderByDescending(p => p.CreatedAt) // Lấy giao dịch mới nhất (nếu có nhiều lần thanh toán)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId && p.Status == Payment.Domain.Enums.PaymentStatus.Success); // Lấy bản ghi đầu tiền hoặc null nếu không tìm thấy
        }

        public async Task<Payment.Domain.Entities.Payment?> GetPendingPaymentByTransactionIdAsync(int transactionId)
        {
            // Lấy giao dịch thanh toán ĐANG CHỜ (Status = Pending) gần nhất
            return await _dbContext.Payments
                //.Where(p => p.TransactionId == transactionId && p.Status == Payment.Domain.Enums.PaymentStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId && p.Status == Payment.Domain.Enums.PaymentStatus.Pending);
        }

        public async Task<IEnumerable<Payment.Domain.Entities.Payment>> GetPaymentsByTransactionIdAsync(int transactionId)
        {
            return await _dbContext.Payments
                .Where(p => p.TransactionId == transactionId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment.Domain.Entities.Payment>> GetAllAsync()
        {
            return await _dbContext.Payments
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
