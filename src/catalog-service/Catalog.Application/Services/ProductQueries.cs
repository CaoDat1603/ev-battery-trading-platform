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
        /// Get all products as product response.
        /// </summary>
        public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductResponse>> SearchBySellerAsync(int sellerId, CancellationToken ct = default)
        {
            var items = await _repo.SearchBySellerAsync(sellerId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductResponse>> SearchModeratedByAsync(int id, CancellationToken ct = default)
        {
            var items = await _repo.SearchModeratedByAsync(id, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductResponse>> SearchByProductIDAsync(int productId, CancellationToken ct = default)
        {
            var items = await _repo.SearchByProductIDAsync(productId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ProductResponse>> GetPagedProductsAsync(
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
            CancellationToken ct = default)
        {
            var (products, totalCount) = await _repo.GetPagedAsync(
                pageNumber, pageSize, sortBy, keyword, minPrice, maxPrice, pickupAddress, status, saleMethod, sellerId, isSpam, isVerified, productType, createAt, updateAt, deleteAt, ct);

            return products.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<int> GetProductCountAsync(
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
            CancellationToken ct = default)
        {
            var count = await _repo.GetProductCountAsync(keyword, minPrice, maxPrice, pickupAddress, sellerId, status, saleMethod, isSpam, isVerified, productType, createAt, updateAt, deleteAt, ct);

            return count;
        }

        private static ProductResponse MapToDto(Product product)
        {
            var detail = product.Details.FirstOrDefault();
            return new ProductResponse
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Price = product.Price,
                SellerId = product.SellerId,
                MethodSale = product.MethodSale,
                StatusProduct = product.StatusProduct,
                PickupAddress = product.PickupAddress,
                ProductName = detail?.ProductName ?? string.Empty,
                Description = detail?.Description ?? string.Empty,
                ProductType = detail.ProductType,
                RegistrationCard = detail?.RegistrationCard,
                IsSpam = product.IsSpam,
                IsVerified = product.IsVerified,
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
