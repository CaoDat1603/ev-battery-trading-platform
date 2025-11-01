using Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Application.DTOs
{
    //    public class ProductBriefDto(
    //        int ProductId, string Title, decimal Price, ProductStatus StatusProduct);

    public class ProductResponse
    {
        [Required] public int ProductId { get; set; }
        [Required] public string Title { get; set; } = default!;
        [Required] public decimal Price { get; set; }
        [Required] public int SellerId { get; set; }
        [Required] public ProductStatus StatusProduct { get; set; }
        [Required] public string PickupAddress { get; set; } = default!;
        [Required] public string ProductName { get; set; } = default!;
        [Required] public string Description { get; set; } = default!;
        [Required] public int ProductType { get; set; }
        public string? RegistrationCard { get; set; }
        public string? FileUrl { get; set; }
        public string? ImageUrl { get; set; }
        public int? ModeratedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}