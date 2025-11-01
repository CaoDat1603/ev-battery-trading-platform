using Auction.Application.Contracts;
using Auction.Application.DTOs;
using Auction.Domain.Abstractions;
using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Application.Services
{
    /// <summary>
    /// Query service for retrieving auction information.
    /// </summary>
    public class AuctionQueries : IAuctionQueries
    {
        private readonly IAuctionRepository _repo;

        public AuctionQueries(IAuctionRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Retrieves all auctions that have not been deleted.
        /// </summary>
        public async Task<IReadOnlyList<AuctionResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        /// <summary>
        /// Searches auctions by their unique Auction ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionResponse>> SearchByAuctionIDAsync(int auctionId, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(auctionId, ct);
            if (item == null)
                return Array.Empty<AuctionResponse>();

            return new List<AuctionResponse> { MapToDto(item) }.AsReadOnly();
        }

        /// <summary>
        /// Searches auctions by associated product ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionResponse>> SearchByProductAsync(int productId, CancellationToken ct = default)
        {
            var items = await _repo.SearchByProductAsync(productId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        /// <summary>
        /// Searches auctions by winner ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionResponse>> SearchByWinnerAsync(int winnerId, CancellationToken ct = default)
        {
            var items = await _repo.SearchByWinnerAsync(winnerId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        /// <summary>
        /// Searches auctions by transaction ID.
        /// </summary>
        public async Task<IReadOnlyList<AuctionResponse>> SearchByTransactionAsync(int transactionId, CancellationToken ct = default)
        {
            var items = await _repo.SearchByTransactionAsync(transactionId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        /// <summary>
        /// Retrieves paged auctions with filtering and sorting options.
        /// </summary>
        public async Task<IReadOnlyList<AuctionResponse>> GetPagedAsync(
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
            CancellationToken ct = default)
        {
            var (auctions, totalCount) = await _repo.GetPagedAsync(
                pageNumber, pageSize, sortBy,
                productId, winnerId, sellerEmail, sellerPhone, transactionId,
                minPrice, maxPrice, startTime, endTime, status, ct);

            return auctions.Select(MapToDto).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets total count of auctions matching given filters.
        /// </summary>
        public async Task<int> GetAuctionCountAsync(
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
            var (auctions, totalCount) = await _repo.GetPagedAsync(
                1, 1, "newest",
                productId, winnerId, sellerEmail, sellerPhone, transactionId,
                minPrice, maxPrice, startTime, endTime, status, ct);

            return totalCount;
        }

        /// <summary>
        /// Maps an Auction entity to an AuctionResponse DTO.
        /// </summary>
        private static AuctionResponse MapToDto(AuctionItem auction)
        {
            return new AuctionResponse
            {
                AuctionId = auction.AuctionId,
                ProductId = auction.ProductId,
                SellerEmail = auction.SellerEmail,
                SellerPhone = auction.SellerPhone,
                WinnerId = auction.WinnerId,
                TransactionId = auction.TransactionId,
                StartingPrice = auction.StartingPrice,
                CurrentPrice = auction.CurrentPrice,
                DepositAmount = auction.DepositAmount,
                Status = auction.Status,
                StartTime = auction.StartTime,
                CreatedAt = auction.CreatedAt,
                UpdatedAt = auction.UpdatedAt,
                DeletedAt = auction.DeletedAt
            };
        }
    }
}
