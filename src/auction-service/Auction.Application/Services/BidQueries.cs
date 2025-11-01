using Auction.Application.Contracts;
using Auction.Application.DTOs;
using Auction.Domain.Abstractions;
using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Application.Services
{
    /// <summary>
    /// Provides read/query operations for Bid data.
    /// </summary>
    public class BidQueries : IBidQueries
    {
        private readonly IBidRepository _repo;

        public BidQueries(IBidRepository repo)
        {
            _repo = repo;
        }

        // ==========================================
        // BASIC QUERIES
        // ==========================================

        public async Task<IReadOnlyList<BidResponse>> GetAllAsync(CancellationToken ct = default)
            => (await _repo.GetAllAsync(ct))
                .Select(MapToDto)
                .ToList()
                .AsReadOnly();

        public async Task<IReadOnlyList<BidResponse>> SearchByBidIDAsync(int bidId, CancellationToken ct = default)
        {
            var bid = await _repo.GetByIdAsync(bidId, ct);
            if (bid == null) return Array.Empty<BidResponse>();
            return new List<BidResponse> { MapToDto(bid) }.AsReadOnly();
        }


        public async Task<IReadOnlyList<BidResponse>> SearchByAuctionAsync(int auctionId, CancellationToken ct = default)
            => (await _repo.SearchByAuctionAsync(auctionId, ct))
                .Select(MapToDto)
                .ToList()
                .AsReadOnly();

        public async Task<IReadOnlyList<BidResponse>> SearchByBidderAsync(int bidderId, CancellationToken ct = default)
            => (await _repo.SearchByBidderAsync(bidderId, ct))
                .Select(MapToDto)
                .ToList()
                .AsReadOnly();

        // ==========================================
        // ADVANCED QUERIES
        // ==========================================

        /// <summary>
        /// Returns a paginated list of bids with optional filters.
        /// </summary>
        public async Task<IReadOnlyList<BidResponse>> GetPagedBidsAsync(
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
            var (bids, _) = await _repo.GetPagedAsync(
                pageNumber, pageSize, sortBy,
                auctionId, bidderId,
                minAmount, maxAmount,
                placedAfter, placedBefore,
                statusDeposit, isWinning, ct);

            return bids.Select(MapToDto).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the total count of bids that match specific filters.
        /// </summary>
        public Task<int> GetBidCountAsync(
            int? auctionId = null,
            int? bidderId = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTimeOffset? placedAfter = null,
            DateTimeOffset? placedBefore = null,
            DepositStatus? statusDeposit = null,
            bool? isWinning = null,
            CancellationToken ct = default)
            => _repo.GetBidCountAsync(
                auctionId, bidderId, minAmount, maxAmount,
                placedAfter, placedBefore, statusDeposit, isWinning, ct);

        // ==========================================
        // MAPPER
        // ==========================================

        private static BidResponse MapToDto(Bid bid) => new()
        {
            BidId = bid.BidId,
            AuctionId = bid.AuctionId,
            BidderId = bid.BidderId,
            BidderEmail = bid.BidderEmail,
            BidderPhone = bid.BidderPhone,
            BidAmount = bid.BidAmount,
            StatusDeposit = bid.StatusDeposit,
            IsWinning = bid.IsWinning,
            CreatedAt = bid.CreatedAt,
            UpdatedAt = bid.UpdatedAt,
            DeletedAt = bid.DeletedAt
        };
    }
}
