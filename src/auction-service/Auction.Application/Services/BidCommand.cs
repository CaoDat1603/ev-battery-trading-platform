using Auction.Application.Contracts;
using Auction.Application.DTOs;
using Auction.Domain.Abstractions;
using Auction.Application.Abstractions;
using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Application.Services
{
    public class BidCommand : IBidCommand
    {
        private readonly IAuctionRepository _auctionRepo;
        private readonly IBidRepository _bidRepo;
        private readonly IUnitOfWork _uow;
        private readonly IOrderClient _orderClient;

        public BidCommand(IAuctionRepository auctionRepo, IBidRepository bidRepo, IUnitOfWork uow, IOrderClient orderClient)
        {
            _auctionRepo = auctionRepo;
            _bidRepo = bidRepo;
            _uow = uow;
            _orderClient = orderClient;
        }

        /// <summary>
        /// Places a new bid on an active auction.
        /// </summary>
        public async Task<int> PlaceBidAsync(PlaceBidDto dto, CancellationToken ct = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var auction = await _auctionRepo.GetByIdAsync(dto.AuctionId, ct);
            if (auction == null)
                throw new InvalidOperationException($"Auction with ID {dto.AuctionId} does not exist.");

            if (dto.TransactionId != null)
            {
                var ok = await _orderClient.IsTransactionCompleted(dto.TransactionId, auction.DepositAmount, ct);
                if (!ok)
                {
                    throw new InvalidOperationException($"Transaction with ID {dto.TransactionId} does not exist.");
                }
            }

            // Call domain logic to place a bid
            var bid = auction.PlaceBid(
                dto.BidderId,
                dto.BidderEmail,
                dto.BidderPhone,
                dto.Amount,
                dto.TransactionId
            );

            await _bidRepo.AddAsync(bid, ct);
            await _auctionRepo.UpdateAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);

            return bid.BidId;
        }

        /// <summary>
        /// Updates the deposit status (Paid, Refunded, Forfeited, etc.).
        /// </summary>
        public async Task<bool> UpdateBidStatusAsync(int bidId, DepositStatus newStatus, CancellationToken ct = default)
        {
            var bid = await _bidRepo.GetByIdAsync(bidId, ct);
            if (bid == null) return false;

            switch (newStatus)
            {
                case DepositStatus.Paid:
                    bid.MarkDepositAsPaid();
                    break;
                case DepositStatus.Refunded:
                    bid.RefundDeposit();
                    break;
                case DepositStatus.Forfeited:
                    bid.ForfeitDeposit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newStatus), "Invalid deposit status.");
            }

            await _bidRepo.UpdateAsync(bid, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Updates bidder contact information (email or phone).
        /// </summary>
        public async Task<bool> UpdateContactInfoAsync(int bidId, string? newEmail, string? newPhone, CancellationToken ct = default)
        {
            var bid = await _bidRepo.GetByIdAsync(bidId, ct);
            if (bid == null) return false;

            bid.UpdateContactInfo(newEmail, newPhone);

            await _bidRepo.UpdateAsync(bid, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Marks a bid as winning or forfeits it if the winner withdraws.
        /// </summary>
        public async Task<bool> UpdateWinningBidAsync(int bidId, bool isWinning, CancellationToken ct = default)
        {
            var bid = await _bidRepo.GetByIdAsync(bidId, ct);
            if (bid == null) return false;

            if (isWinning)
                bid.MarkAsWinning();
            else
                bid.ForfeitDeposit(); // If revoked, treat it as deposit forfeited

            await _bidRepo.UpdateAsync(bid, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Updates the bid amount (only allowed if the auction is still active).
        /// </summary>
        public async Task<bool> UpdateBidAmountAsync(int bidId, decimal newAmount, CancellationToken ct = default)
        {
            var bid = await _bidRepo.GetByIdAsync(bidId, ct);
            if (bid == null) return false;

            var auction = await _auctionRepo.GetByIdAsync(bid.AuctionId, ct);
            if (auction == null)
                throw new InvalidOperationException($"Auction with ID {bid.AuctionId} does not exist.");

            bid.UpdateBidAmount(newAmount);
            auction.UpdateCurrentPrice(newAmount);

            await _auctionRepo.UpdateAsync(auction, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateTransactionAsync(int bidId, int transactionId, CancellationToken ct = default)
        {
            var bid = await _bidRepo.GetByIdAsync(bidId, ct);
            if (bid == null) return false;

            await _bidRepo.UpdateTransaction(bidId, transactionId, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Performs a soft delete on a bid (mark as deleted without removing it from the database).
        /// </summary>
        public async Task<bool> DeleteBidAsync(int bidId, CancellationToken ct = default)
        {
            var bid = await _bidRepo.GetByIdAsync(bidId, ct);
            if (bid == null) return false;

            bid.SoftDelete();

            await _bidRepo.UpdateAsync(bid, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}