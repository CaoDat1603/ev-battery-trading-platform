using System.ComponentModel.DataAnnotations;

namespace Complaints.Application.DTOs
{
    public class ProductInfoDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public int SellerId { get; set; }
        public string StatusProduct { get; set; }
        public string PickupAddress { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string ProductType { get; set; }
        public string MethodSale { get; set; }
        public bool IsSpam { get; set; }
        public bool IsVerified { get; set; }
        public string? RegistrationCard { get; set; }
        public string? FileUrl { get; set; }
        public string? ImageUrl { get; set; }
        public int? ModeratedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
