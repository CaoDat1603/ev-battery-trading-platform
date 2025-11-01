using Auction.Application.DTOs;
using Auction.Domain.Enums;

namespace Auction.Application.Contracts
{
    public interface IAuctionQueries
    {
        Task<IReadOnlyList<AuctionResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<AuctionResponse>> SearchByAuctionIDAsync(int auctionId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionResponse>> SearchByProductAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionResponse>> SearchByWinnerAsync(int winnerId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionResponse>> SearchByTransactionAsync(int transactionId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
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
            CancellationToken ct = default);
        Task<int> GetAuctionCountAsync(
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
            CancellationToken ct = default);
    }
}
