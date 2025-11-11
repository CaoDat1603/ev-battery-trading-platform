using Wishlist.Application.DTOs;

namespace Wishlist.Application.Contracts
{
    public interface IWishlistCommands
    {
        Task<int> CreateAsync(CreateWishlistDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int wishlistItemId, CancellationToken ct = default);
    }
}
