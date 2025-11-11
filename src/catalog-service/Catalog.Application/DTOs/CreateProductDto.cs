using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Application.DTOs
{
    public class CreateProductDto
    {
        [Required] public string Title { get; set; } = default!;
        [Required] public decimal Price { get; set; }
        [Required] public ProductType ProductType { get; set; }
        [Required] public int SellerId { get; set; }
        [Required] public string PickupAddress { get; set; } = default!;
        [Required] public string ProductName { get; set; } = default!;
        [Required] public string Description { get; set; } = default!;
        [Required] public SaleMethod saleMethod { get; set; }
        [Required] public bool IsSpam { get; set; } = false;
        public string? RegistrationCard { get; set; }
        public IFormFile? FileUrl { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}