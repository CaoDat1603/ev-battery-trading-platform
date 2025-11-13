using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Domain.Abstractions
{
    public interface IAuctionRepository
    {
        // === CRUD ===
        Task AddAsync(AuctionItem entity, CancellationToken ct = default);
        Task<AuctionItem?> GetByIdAsync(int auctionId, CancellationToken ct = default);
        Task UpdateAsync(AuctionItem auction, CancellationToken ct = default);
        Task SoftDeleteAsync(int auctionId, CancellationToken ct = default);

        // === Queries ===
        Task<IReadOnlyList<AuctionItem>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<AuctionItem>> SearchByProductAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionItem>> SearchByWinnerAsync(int winnerId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionItem>> SearchByTransactionAsync(int transactionId, CancellationToken ct = default);
        Task<(IReadOnlyList<AuctionItem> Auctions, int TotalCount)> GetPagedAsync(
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
            DateTimeOffset? createAt = null,
            DateTimeOffset? updateAt = null,
            DateTimeOffset? deleteAt = null,
            CancellationToken ct = default);

        Task<int> GetCountAsync(
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
            DateTimeOffset? createAt = null,
            DateTimeOffset? updateAt = null,
            DateTimeOffset? deleteAt = null,
            CancellationToken ct = default);
    }
}