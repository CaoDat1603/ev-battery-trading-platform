using Wishlist.Domain.Entities;

namespace Wishlist.Domain.Abstractions
{
    public interface IWishlistRepository
    {
        // === CRUD ===
        Task<WishlistItem?> GetByIdAsync(int wishlistId, CancellationToken ct = default);
        Task AddAsync(WishlistItem wishlistItem, CancellationToken ct = default);
        //void Update(WishlistItem wishlistItem);
        Task SoftDeleteAsync(int id, CancellationToken ct = default);

        // === QUERIES ===
        Task<List<WishlistItem>> GetAllAsync(CancellationToken ct = default);
        Task<List<WishlistItem>> GetItemsByUserIdAsync(int userId, CancellationToken ct = default);

        Task<(IReadOnlyList<WishlistItem>, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            int? userId = null,
            int? productId = null,
            CancellationToken ct = default);

        Task<int> GetWishlistCountAsync(
            int? userId = null,
            int? productId = null,
            CancellationToken ct = default);
    }
}
