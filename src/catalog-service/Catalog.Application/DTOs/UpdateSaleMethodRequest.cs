using Catalog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Application.DTOs
{
    public class UpdateSaleMethodRequest
    {
        [Required] public int ProductId { get; set; }
        [Required] public SaleMethod NewMethod { get; set; }
    }
}
