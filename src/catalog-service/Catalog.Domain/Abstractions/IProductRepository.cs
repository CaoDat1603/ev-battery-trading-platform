using Catalog.Domain.Entities;
using Catalog.Domain.Enums;

namespace Catalog.Domain.Abstractions
{
    public interface IProductRepository
    {
        //Get Product bằng ID
        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<Product>> SearchByProductIDAsync(int id, CancellationToken ct = default);

        //Get tất cả Product
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);

        // Tìm kiếm theo SellerId
        Task<IReadOnlyList<Product>> SearchBySellerAsync(int sellerId, CancellationToken ct = default);

        Task<IReadOnlyList<Product>> SearchWithFiltersAsync(
            string? keyword,
            decimal? minPrice,
            decimal? maxPrice,
            string? pickupAddress,
            ProductStatus? status,
            int take = 50,
            CancellationToken ct = default);

        Task AddAsync(Product entity, CancellationToken ct = default);
        void Update(Product product);
    }
}
