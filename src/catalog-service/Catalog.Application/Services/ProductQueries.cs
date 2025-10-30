using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Abstractions;
using Catalog.Domain.Enums;

namespace Catalog.Application.Services
{
    public class ProductQueries : IProductQueries
    {
        private readonly IProductRepository _repo;
        public ProductQueries(IProductRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<ProductBriefDto>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return items.Select(p => new ProductBriefDto(p.ProductId, p.Title, p.Price, p.StatusProduct)).ToList();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> SearchBySellerAsync(int sellerId, CancellationToken ct = default)
        {
            var products = await _repo.SearchBySellerAsync(sellerId, ct);
            return products.Select(p => new ProductBriefDto(p.ProductId, p.Title, p.Price, p.StatusProduct)).ToList();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> SearchByProductIDAsync(int productId, CancellationToken ct = default)
        {
            var products = await _repo.SearchByProductIDAsync(productId, ct);
            return products.Select(p => new ProductBriefDto(p.ProductId, p.Title, p.Price, p.StatusProduct)).ToList();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> GetPagedProductsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? keyword = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            ProductStatus? status = null,
            int? sellerId = null,
            CancellationToken ct = default)
        {
            var (products, totalCount) = await _repo.GetPagedAsync(
                pageNumber,
                pageSize,
                keyword,
                minPrice,
                maxPrice,
                pickupAddress,
                status,
                sellerId,
                ct);

            // Map domain entities to brief DTOs
            var result = products.Select(p =>
                new ProductBriefDto(
                    p.ProductId,
                    p.Title,
                    p.Price,
                    p.StatusProduct))
                .ToList()
                .AsReadOnly();

            return result;
        }


    }
}
