using Catalog.Domain.Enums;

namespace Catalog.Application.DTOs
{
    public record ProductBriefDto(int ProductId, string Title, decimal Price, ProductStatus StatusProduct);
}
