using Auction.Domain.Abstractions;
using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AppDbContext _db;
        public AuctionRepository(AppDbContext db) => _db = db;

        public Task AddAsync(AuctionItem entity, CancellationToken ct = default)
            => _db.Auctions.AddAsync(entity, ct).AsTask();

        public Task<AuctionItem?> GetByIdAsync(int auctionId, CancellationToken ct = default)
            => _db.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId, ct);

        public async Task<IReadOnlyList<AuctionItem>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Auctions
                .Include(a => a.Bids)
                .OrderByDescending(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<AuctionItem>> SearchByProductAsync(int productId, CancellationToken ct = default)
        {
            return await _db.Auctions
                .Where(a => a.ProductId == productId)
                .Include(a => a.Bids)
                .OrderByDescending(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public void Update(AuctionItem auction)
            => _db.Auctions.Update(auction);
    }
}
