using Wishlist.Application.Contracts;
using Wishlist.Application.DTOs;
using Wishlist.Domain.Abstractions;
using Wishlist.Domain.Entities;
using Wishlist.Infrastructure.Repositories;

namespace Wishlist.Application.Services
{
    /// <summary>
    /// Query service for retrieving wishlist information.
    /// </summary>
    public class WishlistQueries : IWishlistQueries
    {
        private readonly IWishlistRepository _repo;

        public WishlistQueries(IWishlistRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<WishlistResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<WishlistResponse>> SearchByUserIdAsync(int userId, CancellationToken ct = default)
        {
            var items = await _repo.GetItemsByUserIdAsync(userId, ct);
            return items.Select(MapToDto).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<WishlistResponse>> SearchByWishlistIdAsync(int wishlistId, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(wishlistId, ct);
            if (item == null)
                return Array.Empty<WishlistResponse>();
            return new List<WishlistResponse> { MapToDto(item) }.AsReadOnly();
        }

        public async Task<IReadOnlyList<WishlistResponse>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            int? userId = null,
            int? productId = null,
            CancellationToken ct = default)
        {
            var (items, totalCount) = await _repo.GetPagedAsync(pageNumber, pageSize, sortBy, userId, productId, ct);
            var dtoItems = items.Select(MapToDto).ToList().AsReadOnly();
            return dtoItems;
        }

        public async Task<int> GetWishlistCountAsync(
            int? userId = null,
            int? productId = null,
            CancellationToken ct = default)
        {
            var count = await _repo.GetWishlistCountAsync(userId, productId, ct);
            return count;
        }

        private static WishlistResponse MapToDto(WishlistItem item)
        {
            return new WishlistResponse
            {
                WishlistId = item.WishlistId,
                UserId = item.UserId,
                ProductId = item.ProductId,
                CreatedAt = item.CreatedAt
            };
        }
    }
}
