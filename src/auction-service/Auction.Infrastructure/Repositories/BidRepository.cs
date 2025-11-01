using Auction.Domain.Abstractions;
using Auction.Domain.Entities;
using Auction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing Bid entities.
    /// Supports CRUD, soft deletion, and advanced search/filter queries.
    /// </summary>
    public class BidRepository : IBidRepository
    {
        private readonly AppDbContext _db;
        public BidRepository(AppDbContext db) => _db = db;

        // === CRUD ===

        /// <summary>
        /// Adds a new Bid entity to the database.
        /// </summary>
        public async Task AddAsync(Bid entity, CancellationToken ct = default)
            => await _db.Bids.AddAsync(entity, ct);

        /// <summary>
        /// Gets a Bid by its ID.
        /// </summary>
        public async Task<Bid?> GetByIdAsync(int bidId, CancellationToken ct = default)
            => await _db.Bids
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BidId == bidId && x.DeletedAt == null, ct);

        /// <summary>
        /// Updates an existing Bid entity.
        /// </summary>
        public async Task UpdateAsync(Bid bid, CancellationToken ct = default)
        {
            _db.Bids.Update(bid);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Soft deletes a Bid (marks DeletedAt).
        /// </summary>
        public async Task SoftDeleteAsync(int bidId, CancellationToken ct = default)
        {
            var bid = await _db.Bids.FirstOrDefaultAsync(x => x.BidId == bidId && x.DeletedAt == null, ct);
            if (bid == null) return;

            bid.SoftDelete();
            _db.Bids.Update(bid);
        }

        // === Queries ===

        /// <summary>
        /// Retrieves all non-deleted bids.
        /// </summary>
        public async Task<IReadOnlyList<Bid>> GetAllAsync(CancellationToken ct = default)
            => await _db.Bids
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .ToListAsync(ct);

        /// <summary>
        /// Retrieves all bids by auction ID.
        /// </summary>
        public async Task<IReadOnlyList<Bid>> SearchByAuctionAsync(int auctionId, CancellationToken ct = default)
            => await _db.Bids
                .AsNoTracking()
                .Where(x => x.AuctionId == auctionId && x.DeletedAt == null)
                .OrderByDescending(x => x.BidAmount)
                .ToListAsync(ct);

        /// <summary>
        /// Retrieves all bids by bidder ID.
        /// </summary>
        public async Task<IReadOnlyList<Bid>> SearchByBidderAsync(int bidderId, CancellationToken ct = default)
            => await _db.Bids
                .AsNoTracking()
                .Where(x => x.BidderId == bidderId && x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

        /// <summary>
        /// Retrieves a paged and filtered list of bids.
        /// </summary>
        public async Task<(IReadOnlyList<Bid> Bids, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = "highest",
            int? auctionId = null,
            int? bidderId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTimeOffset? placedAfter = null,
            DateTimeOffset? placedBefore = null,
            DepositStatus? statusDeposit = null,
            bool? isWinning = null,
            CancellationToken ct = default)
        {
            var query = _db.Bids
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            // === Filters ===
            if (auctionId.HasValue)
                query = query.Where(x => x.AuctionId == auctionId.Value);

            if (bidderId.HasValue)
                query = query.Where(x => x.BidderId == bidderId.Value);

            if (minAmount.HasValue)
                query = query.Where(x => x.BidAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(x => x.BidAmount <= maxAmount.Value);

            if (placedAfter.HasValue)
                query = query.Where(x => x.CreatedAt >= placedAfter.Value);

            if (placedBefore.HasValue)
                query = query.Where(x => x.CreatedAt <= placedBefore.Value);

            if (statusDeposit.HasValue)
                query = query.Where(x => x.StatusDeposit == statusDeposit.Value);

            if (isWinning.HasValue)
                query = query.Where(x => x.IsWinning == isWinning.Value);

            // === Sorting ===
            query = sortBy?.ToLower() switch
            {
                "lowest" => query.OrderBy(x => x.BidAmount),
                "newest" => query.OrderByDescending(x => x.CreatedAt),
                "oldest" => query.OrderBy(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.BidAmount) // default "highest"
            };

            // === Paging ===
            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        /// <summary>
        /// Count Bid
        /// </summary>
        public async Task<int> GetBidCountAsync(
            int? auctionId = null,
            int? bidderId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTimeOffset? placedAfter = null,
            DateTimeOffset? placedBefore = null,
            DepositStatus? statusDeposit = null,
            bool? isWinning = null,
            CancellationToken ct = default)
        {
            var query = _db.Bids
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .AsQueryable();

            if (auctionId.HasValue)
                query = query.Where(b => b.AuctionId == auctionId.Value);

            if (bidderId.HasValue)
                query = query.Where(b => b.BidderId == bidderId.Value);

            if (minAmount.HasValue)
                query = query.Where(b => b.BidAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(b => b.BidAmount <= maxAmount.Value);

            if (placedAfter.HasValue)
                query = query.Where(b => b.CreatedAt >= placedAfter.Value);

            if (placedBefore.HasValue)
                query = query.Where(b => b.CreatedAt <= placedBefore.Value);

            if (statusDeposit.HasValue)
                query = query.Where(b => b.StatusDeposit == statusDeposit.Value);

            if (isWinning.HasValue)
                query = query.Where(b => b.IsWinning == isWinning.Value);

            return await query.CountAsync(ct);
        }
    }
}
