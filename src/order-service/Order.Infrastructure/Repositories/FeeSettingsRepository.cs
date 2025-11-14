using Order.Domain.Abstraction;
using Order.Domain.Entities;
using Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Repositories
{
    public class FeeSettingsRepository : IFeeSettingsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public FeeSettingsRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FeeSettings> GetActiveFeeSettingsAsync(int productType)
        {
            return await _dbContext.FeeSettings
                .Where(fs => fs.Type == productType && fs.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateFeeSettingsAsync(FeeSettings newSettings)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTimeOffset.UtcNow;

                // Deactivate existing active settings for the same type
                await _dbContext.FeeSettings
                    .Where(fs => fs.Type == newSettings.Type && fs.IsActive)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(b => b.IsActive, _ => false)
                        .SetProperty(b => b.EndedDate, _ => now));

                // Activate and insert new settings
                newSettings.Active();
                await _dbContext.FeeSettings.AddAsync(newSettings);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<FeeSettings>> GetHistoryAsync()
        {
            return await _dbContext.FeeSettings
                .OrderByDescending(f => f.EffectiveDate)
                .ThenByDescending(f => f.FeeId)
                .ToListAsync();
        }
    }
}