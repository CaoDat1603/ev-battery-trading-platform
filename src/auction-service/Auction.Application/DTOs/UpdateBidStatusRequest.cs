using System.ComponentModel.DataAnnotations;
using Auction.Domain.Enums;

namespace Auction.Application.DTOs
{
    public class UpdateBidStatusRequest
    {
        [Required] public int BidId { get; set; }
        [Required] public DepositStatus NewStatus { get; set; }
    }
}
