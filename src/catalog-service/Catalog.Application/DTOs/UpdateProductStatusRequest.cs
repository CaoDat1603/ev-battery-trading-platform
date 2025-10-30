using Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Application.DTOs
{
    public class UpdateProductStatusRequest
    {
        [Required] public int ProductId { get; set; }
        [Required] public ProductStatus NewStatus { get; set; }
    }
}
