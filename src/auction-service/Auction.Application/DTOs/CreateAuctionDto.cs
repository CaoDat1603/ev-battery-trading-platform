using System.ComponentModel.DataAnnotations;

namespace Auction.Application.DTOs
{
    public class CreateAuctionDto
    {
        [Required] public int ProductId { get; set; }
        [Required] public decimal StartingPrice { get; set; }
        [Required] public DateTimeOffset StartTime { get; set; }
        [Required] public DateTimeOffset EndTime { get; set; }
        public string? SellerEmail { get; set; }
        public string? SellerPhone { get; set; }
    }
}
