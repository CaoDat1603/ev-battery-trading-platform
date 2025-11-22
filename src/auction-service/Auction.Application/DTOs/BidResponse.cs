using Auction.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auction.Application.DTOs
{
    public class BidResponse
    {
        [Required] public int BidId { get; set; }
        [Required] public int AuctionId { get; set; }
        [Required] public int BidderId { get; set; }
        public string? BidderEmail { get; set; }
        public string? BidderPhone { get; set; }
        public int? TransactionId { get; set; }

        [Required] public decimal BidAmount { get; set; }
        public DepositStatus StatusDeposit { get; set; }
        public bool IsWinning { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
