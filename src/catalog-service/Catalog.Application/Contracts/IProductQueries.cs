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
           SaleMethod? saleMethod = null,
           int? sellerId = null,
           bool? isSpam = null,
           bool? isVerified = null,
           ProductType? productType = null,
           DateTimeOffset? createAt = null,
           DateTimeOffset? updateAt = null,
           DateTimeOffset? deleteAt = null,
           CancellationToken ct = default);

        Task<int> GetProductCountAsync(
            string? keyword = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            int? sellerId = null,
            ProductStatus? status = null,
            SaleMethod? saleMethod = null,
            bool? isSpam = null,
            bool? isVerified = null,
            ProductType? productType = null,
            DateTimeOffset? createAt = null,
            DateTimeOffset? updateAt = null,
            DateTimeOffset? deleteAt = null,
            CancellationToken ct = default);
    }
}

