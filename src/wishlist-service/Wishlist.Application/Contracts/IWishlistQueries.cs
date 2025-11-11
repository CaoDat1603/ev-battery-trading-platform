using Wishlist.Application.DTOs;

namespace Wishlist.Application.Contracts
{
    public interface IWishlistQueries
    {
        Task<IReadOnlyList<WishlistResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<WishlistResponse>> SearchByWishlistIdAsync(int wishlistId, CancellationToken ct = default);
        Task<IReadOnlyList<WishlistResponse>> SearchByUserIdAsync(int userId, CancellationToken ct = default);
        Task<IReadOnlyList<WishlistResponse>> GetPagedAsync(
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
