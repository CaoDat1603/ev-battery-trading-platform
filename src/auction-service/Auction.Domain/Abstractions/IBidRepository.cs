using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Domain.Abstractions
{
    public interface IBidRepository
    {
        // === CRUD ===
        Task AddAsync(Bid entity, CancellationToken ct = default);
        Task<Bid?> GetByIdAsync(int bidId, CancellationToken ct = default);
        Task UpdateAsync(Bid bid, CancellationToken ct = default);
        Task SoftDeleteAsync(int bidId, CancellationToken ct = default);

        // === Queries ===
        Task<IReadOnlyList<Bid>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Bid>> SearchByAuctionAsync(int auctionId, CancellationToken ct = default);
        Task<IReadOnlyList<Bid>> SearchByBidderAsync(int bidderId, CancellationToken ct = default);
        Task<(IReadOnlyList<Bid> Bids, int TotalCount)> GetPagedAsync(
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
            DateTimeOffset? createAt = null,
            DateTimeOffset? updateAt = null,
            DateTimeOffset? deleteAt = null,
            CancellationToken ct = default);
        Task<int> GetBidCountAsync(
            int? auctionId = null,
            int? bidderId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTimeOffset? placedAfter = null,
            DateTimeOffset? placedBefore = null,
            DepositStatus? statusDeposit = null,
            bool? isWinning = null,
            DateTimeOffset? createAt = null,
            DateTimeOffset? updateAt = null,
            DateTimeOffset? deleteAt = null,
            CancellationToken ct = default);
    }
}
