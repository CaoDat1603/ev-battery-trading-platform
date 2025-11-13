using Auction.Application.Contracts;
using Auction.Application.DTOs;
using Auction.Domain.Abstractions;
using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Application.Services
{
    /// <summary>
    /// Handles all auction-related commands (create, update, delete, etc.).
    /// Coordinates domain logic and persistence.
    /// </summary>
    public class AuctionCommand : IAuctionCommand
    {
        private readonly IAuctionRepository _repo;
        private readonly IBidRepository _bidRepo;
        private readonly IUnitOfWork _uow;

        public AuctionCommand(IAuctionRepository repo, IBidRepository bidRepo, IUnitOfWork uow)
        {
            _repo = repo;
            _bidRepo = bidRepo;
            _uow = uow;
        }

        /// <summary>
        /// Creates a new auction for a given product.
        /// </summary>
        public async Task<int> CreateAuctionAsync(CreateAuctionDto dto, CancellationToken ct = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Create domain entity
            var auction = AuctionItem.Create(
                productId: dto.ProductId,
                startingPrice: dto.StartingPrice,
                sellerEmail: dto.SellerEmail ?? string.Empty,
                sellerPhone: dto.SellerPhone ?? string.Empty,
                startTime: dto.StartTime,
                endTime: dto.EndTime
            );

            // Save to repository
            await _repo.AddAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);

            return auction.AuctionId;
        }

        /// <summary>
        /// Updates the status of an auction (e.g., start, end, complete, cancel).
        /// </summary>
        public async Task<bool> UpdateAuctionStatusAsync(int auctionId, AuctionStatus newStatus, CancellationToken ct = default)
        {
            var auction = await _repo.GetByIdAsync(auctionId, ct);
            if (auction == null) return false;

            switch (newStatus)
            {
                case AuctionStatus.Active:
                    auction.StartAuction();
                    break;

                case AuctionStatus.Ended:
                    auction.EndAuction();
                    break;

                case AuctionStatus.Completed:
                    // Completed requires transaction ID — handled elsewhere
                    throw new InvalidOperationException("Use CompleteAuction() with transaction ID.");

                case AuctionStatus.Cancelled:
                    auction.CancelAuction();
                    break;

                default:
                    throw new ArgumentException("Invalid auction status transition.");
            }

            await _repo.UpdateAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        /// <summary>
        /// Updates the current price manually (e.g., for admin correction or bid adjustment).
        /// </summary>
        public async Task<bool> UpdateCurrentPrice(int auctionId, decimal newPrice, CancellationToken ct = default)
        {
            var auction = await _repo.GetByIdAsync(auctionId, ct);
            if (auction == null) return false;

            auction.UpdateCurrentPrice(newPrice);
            await _repo.UpdateAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        /// <summary>
        /// Updates the seller's contact info (email, phone).
        /// </summary>
        public async Task<bool> UpdateSellerContact(int auctionId, string? newEmail, string? newPhone, CancellationToken ct = default)
        {
            var auction = await _repo.GetByIdAsync(auctionId, ct);
            if (auction == null) return false;

            auction.UpdateSellerContact(newEmail ?? auction.SellerEmail!, newPhone ?? auction.SellerPhone!);
            await _repo.UpdateAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        /// <summary>
        /// Updates the status to Completed and records the transaction ID.
        /// </summary>
        public async Task<bool> CompleteAuctionAsync(int auctionId, int transactionId, CancellationToken ct = default)
        {
            var auction = await _repo.GetByIdAsync(auctionId, ct);
            if (auction == null)
                throw new InvalidOperationException($"Auction with ID {auctionId} not found.");

            if (auction.Status != AuctionStatus.Ended)
                throw new InvalidOperationException("Auction must be ended before completion.");

            auction.CompleteAuction(transactionId);

            await _repo.UpdateAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        /// <summary>
        /// Performs a soft delete (mark as deleted).
        /// </summary>
        public async Task<bool> DeleteAuctionAsync(int auctionId, CancellationToken ct = default)
        {
            if (auctionId <= 0)
                throw new ArgumentOutOfRangeException(nameof(auctionId));

            var auction = await _repo.GetByIdAsync(auctionId, ct);
            if (auction == null) return false;

            await _repo.SoftDeleteAsync(auctionId, ct);
            // Soft delete related bids
            var bids = await _bidRepo.SearchByAuctionAsync(auctionId, ct);
            foreach (var bid in bids)
            {
                await _bidRepo.SoftDeleteAsync(bid.BidId, ct);
            }
            await _uow.SaveChangesAsync(ct);

            return true;
        }
    }
}
