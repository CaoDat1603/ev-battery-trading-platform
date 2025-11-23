using System.ComponentModel.DataAnnotations;

namespace Auction.Application.DTOs
{
    public class PlaceBidDto
    {
        [Required] public int AuctionId { get; set; }
        [Required] public int BidderId { get; set; }
        [Required] public decimal Amount { get; set; }
        public string? BidderEmail { get; set;}
        public string? BidderPhone { get; set; }

        public int? TransactionId { get; set; }
    }
}
