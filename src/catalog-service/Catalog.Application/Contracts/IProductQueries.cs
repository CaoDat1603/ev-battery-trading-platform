using Catalog.Application.DTOs;
using Catalog.Domain.Enums;

namespace Catalog.Application.Contracts
{
    public interface IProductQueries
    {
        Task<IReadOnlyList<ProductBriefDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> SearchByProductIDAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> SearchBySellerAsync(int sellerId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> SearchModeratedByAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<ProductBriefDto>> GetPagedProductsAsync(
           int pageNumber = 1,
           int pageSize = 20,
           string? sortBy = "newest",
           string? keyword = null,
           decimal? minPrice = null,
           decimal? maxPrice = null,
           string? pickupAddress = null,
           ProductStatus? status = null,
           int? sellerId = null,
           CancellationToken ct = default);

        Task<int> GetProductCountAsync(
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            int? sellerId = null,
            ProductStatus? status = null,
            CancellationToken ct = default);
    }
}

