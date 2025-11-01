using Auction.Domain.Abstractions;
using Auction.Domain.Entities;
using Auction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing Auction entities.
    /// Handles CRUD operations, soft deletion, and advanced search/filter logic.
    /// </summary>
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AppDbContext _db;
        public AuctionRepository(AppDbContext db) => _db = db;

        // =============================
        // CRUD OPERATIONS
        // =============================

        /// <summary>
        /// Adds a new auction entity to the database context.
        /// </summary>
        public async Task AddAsync(AuctionItem entity, CancellationToken ct = default)
            => await _db.Auctions.AddAsync(entity, ct).AsTask();

        /// <summary>
        /// Retrieves an auction by its ID, including its associated bids.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the auction cannot be found.</exception>
        public async Task<AuctionItem?> GetByIdAsync(int auctionId, CancellationToken ct = default)
        {
            var auction = await _db.Auctions
                .Include(a => a.Bids)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId && a.DeletedAt == null, ct);

            if (auction == null)
                throw new KeyNotFoundException($"Auction with ID {auctionId} not found.");

            return auction;
        }

        /// <summary>
        /// Updates an existing auction entity in the database.
        /// </summary>
        public Task UpdateAsync(AuctionItem auction, CancellationToken ct = default)
        {
            _db.Auctions.Update(auction);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs a soft delete by marking the auction as deleted.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the auction does not exist.</exception>
        public async Task SoftDeleteAsync(int auctionId, CancellationToken ct = default)
        {
            var auction = await _db.Auctions
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId && a.DeletedAt == null, ct);

            if (auction == null)
                throw new KeyNotFoundException($"Auction with ID = {auctionId} not found.");

            auction.MarkDeleted();
            _db.Auctions.Update(auction);
        }

        // =============================
        // BASIC QUERIES
        // =============================

        /// <summary>
        /// Retrieves all auctions that have not been deleted.
        /// </summary>
        public async Task<IReadOnlyList<AuctionItem>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Auctions
                .Where(a => a.DeletedAt == null)
                .Include(a => a.Bids)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Searches auctions by product ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionItem>> SearchByProductAsync(int productId, CancellationToken ct = default)
        {
            return await _db.Auctions
                .Where(a => a.ProductId == productId && a.DeletedAt == null)
                .Include(a => a.Bids)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Searches auctions by winner ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionItem>> SearchByWinnerAsync(int winnerId, CancellationToken ct = default)
        {
            return await _db.Auctions
                .Where(a => a.WinnerId == winnerId && a.DeletedAt == null)
                .Include(a => a.Bids)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Searches auctions by transaction ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionItem>> SearchByTransactionAsync(int transactionId, CancellationToken ct = default)
        {
            return await _db.Auctions
                .Where(a => a.TransactionId == transactionId && a.DeletedAt == null)
                .Include(a => a.Bids)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }

        // =============================
        // ADVANCED PAGED SEARCH
        // =============================

        /// <summary>
        /// Retrieves paged auctions with multiple filtering and sorting options.
        /// </summary>
        public async Task<(IReadOnlyList<AuctionItem> Auctions, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = "newest",
            int? productId = null,
            int? winnerId = null,
            string? sellerEmail = null,
            string? sellerPhone = null,
            int? transactionId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            AuctionStatus? status = null,
            CancellationToken ct = default)
        {
            var query = _db.Auctions
                .Include(a => a.Bids)
                .Where(a => a.DeletedAt == null)
                .AsQueryable();

            // ====== FILTERS ======

            if (productId.HasValue)
                query = query.Where(a => a.ProductId == productId.Value);

            if (winnerId.HasValue)
                query = query.Where(a => a.WinnerId == winnerId.Value);

            if (transactionId.HasValue)
                query = query.Where(a => a.TransactionId == transactionId.Value);

            if (!string.IsNullOrWhiteSpace(sellerEmail))
                query = query.Where(a => a.SellerEmail != null &&
                                         EF.Functions.ILike(a.SellerEmail, $"%{sellerEmail}%"));

            if (!string.IsNullOrWhiteSpace(sellerPhone))
                query = query.Where(a => a.SellerPhone != null &&
                                         EF.Functions.ILike(a.SellerPhone, $"%{sellerPhone}%"));

            if (minPrice.HasValue)
                query = query.Where(a => a.CurrentPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.CurrentPrice <= maxPrice.Value);

            if (startTime.HasValue)
                query = query.Where(a => a.StartTime >= startTime.Value);

            if (endTime.HasValue)
                query = query.Where(a => a.EndTime <= endTime.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            // ====== TOTAL COUNT ======
            var totalCount = await query.CountAsync(ct);

            // ====== SORTING ======
            switch (sortBy?.ToLower())
            {
                case "oldest":
                    query = query.OrderBy(a => a.CreatedAt);
                    break;

                case "oldestupdate":
                    query = query.OrderBy(a => a.UpdatedAt);
                    break;

                case "newestupdate":
                    query = query.OrderByDescending(a => a.UpdatedAt);
                    break;

                case "lowestprice":
                    query = query.OrderBy(a => a.CurrentPrice);
                    break;

                case "highestprice":
                    query = query.OrderByDescending(a => a.CurrentPrice);
                    break;

                default:
                    query = query.OrderByDescending(a => a.CreatedAt);
                    break;
            }

            // ====== PAGINATION ======
            var auctions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return (auctions, totalCount);
        }
    }
}
