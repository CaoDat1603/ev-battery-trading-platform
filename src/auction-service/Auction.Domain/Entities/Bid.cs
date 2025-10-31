using Auction.Domain.Enums;
using System;

namespace Auction.Domain.Entities
{
    public class Bid
    {
        public int BidId { get; private set; }
        public int AuctionId { get; private set; }
        public int BidderId { get; private set; }
        public decimal BidAmount { get; private set; }
        public DepositStatus StatusDeposit { get; private set; }
        public bool IsWinning { get; private set; }
        public DateTimeOffset BidTime { get; private set; }

        private Bid() { }

        public static Bid Create(int auctionId, int bidderId, decimal amount)
        {
            return new Bid
            {
                AuctionId = auctionId,
                BidderId = bidderId,
                BidAmount = amount,
                StatusDeposit = DepositStatus.Paid,
                IsWinning = false,
                BidTime = DateTimeOffset.UtcNow
            };
        }

        public void MarkAsWinning()
        {
            IsWinning = true;
        }

        public void RefundDeposit()
        {
            StatusDeposit = DepositStatus.Refunded;
        }

        public void ForfeitDeposit()
        {
            StatusDeposit = DepositStatus.Forfeited;
        }
    }
}
