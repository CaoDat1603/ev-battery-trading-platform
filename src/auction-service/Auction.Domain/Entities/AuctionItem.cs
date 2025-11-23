using Auction.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Auction.Domain.Entities
{
    public class AuctionItem
    {
        [Key]
        public int AuctionId { get; private set; }

        [Required]
        public int ProductId { get; private set; }

        public string? SellerEmail { get; set; }

        public string? SellerPhone { get; set; }

        public int? WinnerId { get; private set; }

        public int? TransactionId { get; private set; }

        [Required]
        public decimal StartingPrice { get; private set; }

        [Required]
        public decimal CurrentPrice { get; private set; }

        [Required]
        public decimal DepositAmount { get; private set; }

        public AuctionStatus Status { get; private set; }

        [Required] public DateTimeOffset StartTime { get; private set; }

        [Required] public DateTimeOffset EndTime { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? DeletedAt { get; private set; }


        private readonly List<Bid> _bids = new();
        public IReadOnlyList<Bid> Bids => _bids;

        private AuctionItem() { }

        /// <summary>
        /// Factory method for creating a new Auction entity.
        /// </summary>
        public static AuctionItem Create(int productId, decimal startingPrice, string sellerEmail, string sellerPhone, DateTimeOffset startTime, DateTimeOffset endTime, decimal depositRate = 0.1m)
        {
            if (startingPrice < 0) 
                throw new ArgumentOutOfRangeException(nameof(startingPrice));
            if (endTime <= startTime) 
                throw new ArgumentException("EndTime must be after StartTime");

            return new AuctionItem
            {
                ProductId = productId,
                SellerEmail = sellerEmail,
                SellerPhone = sellerPhone,
                StartingPrice = startingPrice,
                CurrentPrice = startingPrice,
                DepositAmount = startingPrice * depositRate,
                Status = AuctionStatus.Pending,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Domain methods
        /// </summary>
        public void StartAuction()
        {
            if (Status != AuctionStatus.Pending)
                throw new InvalidOperationException("Auction cannot be started");

            Status = AuctionStatus.Active;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        ///  Ends the auction and determines the winner.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void EndAuction()
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction is not active");

            Status = AuctionStatus.Ended;
            UpdatedAt = DateTimeOffset.UtcNow;

            // Who is the highest bidder?
            var highestBid = _bids.Count > 0 ? _bids[^1] : null; // Get the last bid (highest)
            if (highestBid != null)
            {
                WinnerId = highestBid.BidderId;
                CurrentPrice = highestBid.BidAmount;
                highestBid.MarkAsWinning();
            }
        }

        /// <summary>
        ///  Places a bid on the auction.
        /// </summary>
        /// <param name="bidderId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Bid PlaceBid(int bidderId, string bidderEmail, string bidderPhone,decimal amount, int? transactionId)
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction is not active");

            if (!_bids.Any())
            {
                if (amount < StartingPrice)
                    throw new ArgumentException("Bid amount must be equal or greater than the starting price");
            }
            else
            {
                // There are bids already
                if (amount <= CurrentPrice)
                    throw new ArgumentException("Bid amount must be higher than current price");
            }

            var bid = Bid.Create(AuctionId, bidderId, bidderEmail, bidderPhone, amount, transactionId);
            _bids.Add(bid);
            CurrentPrice = amount;
            UpdatedAt = DateTimeOffset.UtcNow;
            return bid;
        }

        /// <summary>
        /// Changes currentPrice.
        /// </summary>
        public void UpdateCurrentPrice(decimal newPrice)
        {
            if (newPrice < StartingPrice)
                throw new ArgumentOutOfRangeException(nameof(newPrice), "Current price cannot be less than starting price.");
            CurrentPrice = newPrice;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Updates seller contact information
        /// </summary>
        public void UpdateSellerContact(string email, string phone)
        {
            SellerEmail = email;
            SellerPhone = phone;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Changes the auction status.
        /// </summary>
        /// <param name="newStatus"></param>
        private void ChangeStatus(AuctionStatus newStatus)
        {
            if (Status == newStatus)
                return;
            Status = newStatus;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        public void CancelAuction()
        {
            if (Status == AuctionStatus.Completed || Status == AuctionStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel auction");

            ChangeStatus(AuctionStatus.Cancelled);
        }
        public void CompleteAuction(int transactionId)
        {
            if (Status != AuctionStatus.Ended)
                throw new InvalidOperationException("Auction is not ended");
            TransactionId = transactionId;
            ChangeStatus(AuctionStatus.Completed);
        }

        /// <summary>
        /// Soft deletes the auction
        /// </summary>
        public void MarkDeleted()
        {
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
