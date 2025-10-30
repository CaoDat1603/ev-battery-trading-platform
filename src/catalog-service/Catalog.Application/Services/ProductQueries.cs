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

        public async Task<IReadOnlyList<ProductBriefDto>> SearchWithFiltersAsync(
            string? keyword,
            decimal? minPrice,
            decimal? maxPrice,
            string? pickupAddress,
            ProductStatus? status,
            CancellationToken ct = default)
                {
                    var products = await _repo.SearchWithFiltersAsync(keyword, minPrice, maxPrice, pickupAddress, status, 50, ct);
                    return products.Select(p => new ProductBriefDto(p.ProductId, p.Title, p.Price, p.StatusProduct)).ToList();
                }

    }
}
