using Catalog.Application.DTOs;
using Catalog.Domain.Enums;

namespace Catalog.Application.Contracts
{
    public interface IProductQueries
    {
        Task<IReadOnlyList<ProductBriefDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> SearchByProductIDAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> SearchBySellerAsync(int sellerId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> SearchWithFiltersAsync(
            string? keyword,
            decimal? minPrice,
            decimal? maxPrice,
            string? pickupAddress,
            ProductStatus? status,
            CancellationToken ct = default);
    }
}

