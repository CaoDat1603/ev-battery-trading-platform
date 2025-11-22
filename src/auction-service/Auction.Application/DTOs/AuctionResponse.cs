using Auction.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auction.Application.DTOs
{
    public class AuctionResponse
    {
        [Required] public int AuctionId { get; set; }
        [Required] public int ProductId { get; set; }
        public string? SellerEmail { get; set; }
        public string? SellerPhone { get; set; }
        public int? WinnerId { get; set; }
        public int? TransactionId { get; set; }
        [Required] public decimal StartingPrice { get; set; }
        [Required] public decimal CurrentPrice { get; set; }
        [Required] public decimal DepositAmount { get; set; }
        [Required] public AuctionStatus Status { get; set; }
        [Required] public DateTimeOffset StartTime { get; set; }
        [Required] public DateTimeOffset EndTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
