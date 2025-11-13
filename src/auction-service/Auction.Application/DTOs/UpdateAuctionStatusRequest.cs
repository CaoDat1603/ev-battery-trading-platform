using Auction.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auction.Application.DTOs
{
    public class UpdateAuctionStatusRequest
    {
        [Required] public int AuctionId { get; set; }
        [Required] public AuctionStatus AuctionStatus { get; set; }
    }
}
