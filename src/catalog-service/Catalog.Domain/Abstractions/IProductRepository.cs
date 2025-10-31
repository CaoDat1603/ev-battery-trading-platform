using Catalog.Domain.Entities;
using Catalog.Domain.Enums;

namespace Catalog.Domain.Abstractions
{
    public interface IProductRepository
    {
        // === CRUD cơ bản ===
        Task AddAsync(Product entity, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
        Task UpdateAsync(Product product, CancellationToken ct = default);
        Task SoftDeleteAsync(int productId, CancellationToken ct = default);


        Task<IReadOnlyList<Product>> SearchByProductIDAsync(int id, CancellationToken ct = default);

        // Get tất cả Product
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
        // Tìm kiếm theo SellerId
        Task<IReadOnlyList<Product>> SearchBySellerAsync(int sellerId, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> SearchModeratedByAsync(int id, CancellationToken ct = default);
        Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? keyword = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            ProductStatus? status = null,
            int? sellerId = null,
            CancellationToken ct = default);

//        Task<IReadOnlyList<Product>> SearchWithFiltersAsync(
//            string? keyword,
//            decimal? minPrice,
//            decimal? maxPrice,
//            string? pickupAddress,
//            ProductStatus? status,
//            int take = 50,
//            CancellationToken ct = default);
    }
}
