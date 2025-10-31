using Catalog.Application.Contracts;
using Catalog.Application.DTOs;
using Catalog.Domain.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;

namespace Catalog.Application.Services
{
    /// <summary>
    /// Query service for retrieving product information.
    /// </summary>
    public class ProductQueries : IProductQueries
    {
        private readonly IProductRepository _repo;

        public ProductQueries(IProductRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Get all products as brief DTOs.
        /// </summary>
        public async Task<IReadOnlyList<ProductBriefDto>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> SearchBySellerAsync(int sellerId, CancellationToken ct = default)
        {
            var items = await _repo.SearchBySellerAsync(sellerId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> SearchModeratedByAsync(int id, CancellationToken ct = default)
        {
            var items = await _repo.SearchModeratedByAsync(id, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> SearchByProductIDAsync(int productId, CancellationToken ct = default)
        {
            var items = await _repo.SearchByProductIDAsync(productId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductBriefDto>> GetPagedProductsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? sortBy = "newest",
            string? keyword = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            ProductStatus? status = null,
            int? sellerId = null,
            CancellationToken ct = default)
        {
            var (products, totalCount) = await _repo.GetPagedAsync(
                pageNumber, pageSize, sortBy, keyword, minPrice, maxPrice, pickupAddress, status, sellerId, ct);

            return products.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<int> GetProductCountAsync(
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            int? sellerId = null,
            ProductStatus? status = null,
            CancellationToken ct = default)
        {
            var count = await _repo.GetProductCountAsync(minPrice, maxPrice, pickupAddress, sellerId, status, ct);

            return count;
        }


        private static ProductBriefDto MapToDto(Product product)
        {
            var detail = product.Details.FirstOrDefault();
            return new ProductBriefDto
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Price = product.Price,
                SellerId = product.SellerId,
                StatusProduct = product.StatusProduct,
                PickupAddress = product.PickupAddress,
                ProductName = detail?.ProductName ?? string.Empty,
                Description = detail?.Description ?? string.Empty,
                ProductType = detail?.ProductType ?? 0,
                RegistrationCard = detail?.RegistrationCard,
                FileUrl = detail?.FileUrl,
                ImageUrl = detail?.ImageUrl,
                ModeratedBy = product.ModeratedBy,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                DeletedAt = product.DeletedAt
            };
        }
    }
}
