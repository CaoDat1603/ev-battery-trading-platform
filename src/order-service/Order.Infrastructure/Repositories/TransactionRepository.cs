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

        public async Task<Order.Domain.Entities.Transaction?> GetByIdAsync(int id)
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

        public async Task<IEnumerable<Order.Domain.Entities.Transaction>> GetByBuyerIdAsync(int buyerId)
        {
            return await _dbContext.Transactions
                .Where(t => t.BuyerId == buyerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order.Domain.Entities.Transaction>> GetBySellerIdAsync(int sellerId)
        {
            return await _dbContext.Transactions
                .Where(t => t.SellerId == sellerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order.Domain.Entities.Transaction>> GetAllAsync()
        {
            return await _dbContext.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
