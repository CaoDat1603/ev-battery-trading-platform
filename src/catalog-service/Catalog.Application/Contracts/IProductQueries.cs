using Catalog.Application.DTOs;
using Catalog.Domain.Enums;

namespace Catalog.Application.Contracts
{
    public interface IProductQueries
    {
        Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ProductResponse>> SearchByProductIDAsync(int productId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductResponse>> SearchBySellerAsync(int sellerId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductResponse>> SearchModeratedByAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<ProductResponse>> GetPagedProductsAsync(
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

