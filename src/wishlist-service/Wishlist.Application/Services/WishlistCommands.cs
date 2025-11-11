using Wishlist.Application.Contracts;
using Wishlist.Application.DTOs;
using Wishlist.Domain.Abstractions;

namespace Wishlist.Application.Services
{
    public class WishlistCommands : IWishlistCommands
    {
        private readonly IWishlistRepository _repo;
        private readonly IUnitOfWork _uow;

        public WishlistCommands(IWishlistRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<int> CreateAsync(CreateWishlistDto dto, CancellationToken ct = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var wishlistItem = Wishlist.Domain.Entities.WishlistItem.Create(dto.UserId, dto.ProductId);

            await _repo.AddAsync(wishlistItem, ct);
            await _uow.SaveChangesAsync(ct);
            return wishlistItem.WishlistId;
        }

        public async Task<bool> DeleteAsync(int wishlistId, CancellationToken ct = default)
        {
            if (wishlistId <= 0)
                throw new ArgumentException("Invalid wishlist ID.", nameof(wishlistId));

            var wishlistItem = await _repo.GetByIdAsync(wishlistId, ct);
            if (wishlistItem == null)
                return false;

            await _repo.SoftDeleteAsync(wishlistId, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
    }
}
