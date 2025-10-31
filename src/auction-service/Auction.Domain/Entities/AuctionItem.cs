using Auction.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Auction.Domain.Entities
{
    public class AuctionItem
    {
        public int AuctionId { get; private set; }
        public int ProductId { get; private set; }
        public int? WinnerId { get; private set; }
        public int? TransactionId { get; private set; }
        public decimal StartingPrice { get; private set; }
        public decimal CurrentPrice { get; private set; }
        public decimal DepositAmount { get; private set; }
        public AuctionStatus Status { get; private set; }
        public DateTimeOffset StartTime { get; private set; }
        public DateTimeOffset EndTime { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }

        private readonly List<Bid> _bids = new();
        public IReadOnlyList<Bid> Bids => _bids;

        private AuctionItem() { }

        // Factory method
        public static AuctionItem Create(int productId, decimal startingPrice, decimal depositAmount, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (startingPrice < 0) throw new ArgumentOutOfRangeException(nameof(startingPrice));
            if (depositAmount < 0) throw new ArgumentOutOfRangeException(nameof(depositAmount));
            if (endTime <= startTime) throw new ArgumentException("EndTime must be after StartTime");

            return new AuctionItem
            {
                ProductId = productId,
                StartingPrice = startingPrice,
                CurrentPrice = startingPrice,
                DepositAmount = depositAmount,
                Status = AuctionStatus.Pending,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        // Domain methods
        public void StartAuction()
        {
            if (Status != AuctionStatus.Pending)
                throw new InvalidOperationException("Auction cannot be started");

            Status = AuctionStatus.Active;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void EndAuction()
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction is not active");

            Status = AuctionStatus.Ended;
            UpdatedAt = DateTimeOffset.UtcNow;

            // Xác định người thắng nếu có
            var highestBid = _bids.Count > 0 ? _bids[^1] : null; // Lấy bid cuối cùng (giả sử sắp xếp theo BidTime)
            if (highestBid != null)
            {
                WinnerId = highestBid.BidderId;
                CurrentPrice = highestBid.BidAmount;
            }
        }

        public Bid PlaceBid(int bidderId, decimal amount)
        {
            if (Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction is not active");

            if (amount <= CurrentPrice)
                throw new ArgumentException("Bid amount must be higher than current price");

            var bid = Bid.Create(AuctionId, bidderId, amount);
            _bids.Add(bid);
            CurrentPrice = amount;
            UpdatedAt = DateTimeOffset.UtcNow;
            return bid;
        }

        public void CancelAuction()
        {
            if (Status == AuctionStatus.Completed || Status == AuctionStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel auction");

            Status = AuctionStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkDeleted()
        {
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
