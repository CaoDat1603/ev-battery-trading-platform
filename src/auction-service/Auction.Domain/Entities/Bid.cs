using Auction.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Auction.Domain.Entities
{
    public class Bid
    {
        [Key]
        public int BidId { get; private set; }

        [Required]
        public int AuctionId { get; private set; }

        [Required]
        public int BidderId { get; private set; }

        public string? BidderEmail { get; private set; }

        public string? BidderPhone { get; private set; }

        [Required]
        public decimal BidAmount { get; private set; }
        public int? TransactionId { get; private set; }
        public DepositStatus StatusDeposit { get; private set; }

        public bool IsWinning { get; private set; }
        public DateTimeOffset? CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? DeletedAt { get; private set; }

        private Bid() { }

        /// <summary>
        /// Factory method for creating a new Bid entity.
        /// </summary> 
        public static Bid Create(int auctionId, int bidderId, string bidderEmail, string bidderPhone, decimal amount, int transactionId)
        {
            return new Bid
            {
                AuctionId = auctionId,
                BidderId = bidderId,
                BidderEmail = bidderEmail,
                BidderPhone = bidderPhone,
                BidAmount = amount,
                StatusDeposit = DepositStatus.Paid,
                IsWinning = false,
                TransactionId = transactionId,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Updates the bid amount.
        /// </summary>
        /// <param name="newAmount"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void UpdateBidAmount(decimal newAmount)
        {
            if (newAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(newAmount), "Bid amount must be greater than zero.");
            BidAmount = newAmount;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Updates the bidder's contact information.
        /// </summary> 
        public void UpdateContactInfo(string? newEmail, string? newPhone)
        {
            BidderEmail = newEmail;
            BidderPhone = newPhone;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Marks this bid as the winning bid.
        /// </summary>
        public void MarkAsWinning()
        {
            IsWinning = true;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Changes status
        /// </summary>
        /// <param name="newStatus"></param>
        private void ChangeDepositStatus(DepositStatus newStatus)
        {
            if (newStatus == StatusDeposit)
                return;
            StatusDeposit = newStatus;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        public void RefundDeposit() => ChangeDepositStatus(DepositStatus.Refunded);
        public void ForfeitDeposit() => ChangeDepositStatus(DepositStatus.Forfeited);
        public void MarkDepositAsPaid() => ChangeDepositStatus(DepositStatus.Paid);

        public void updateTransaction(int  transactionId)
        {
            TransactionId = transactionId;
        }

        /// <summary>
        /// Deletes the bid (soft delete).
        /// </summary>
        public void SoftDelete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
