using Auction.Application.DTOs;
using Auction.Domain.Enums;

namespace Auction.Application.Contracts
{
    public interface IBidQueries
    {
        Task<IReadOnlyList<BidResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<BidResponse>> SearchByBidIDAsync(int bidId, CancellationToken ct = default);
        Task<IReadOnlyList<BidResponse>> SearchByAuctionAsync(int auctionId, CancellationToken ct = default);
        Task<IReadOnlyList<BidResponse>> SearchByBidderAsync(int bidderId, CancellationToken ct = default);
        Task<IReadOnlyList<BidResponse>> GetPagedBidsAsync(
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
