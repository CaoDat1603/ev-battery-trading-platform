using Order.Domain.Abstraction;
using Order.Domain.Entities;
using Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TransactionRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Order.Domain.Entities.Transaction> GetByIdAsync(int id)
        {
            return await _dbContext.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id);
        }
        public async Task AddAsync(Order.Domain.Entities.Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(Order.Domain.Entities.Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();
        }
    }
}
