using Wishlist.Domain.Abstractions;
using Wishlist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace Wishlist.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing Wishlist entities.
    /// Handles CRUD operations, soft deletion, and advanced search/filter logic.
    /// </summary>
    public class WishlistRepository : IWishlistRepository
    {

        private readonly AppDbContext _dbContext;
        public WishlistRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Add a new wishlist item
        /// </summary>
        /// <param name="wishlistItem"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task AddAsync(WishlistItem wishlistItem, CancellationToken ct = default)
            => await _dbContext.WishlistItems.AddAsync(wishlistItem, ct).AsTask();

        /// <summary>
        /// Get wishlist item by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<WishlistItem?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var wishlist = await _dbContext.WishlistItems
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WishlistId == id, ct);

            if (wishlist == null)
                throw new KeyNotFoundException($"Wishlist item with ID {id} not found.");

            return wishlist;
        }

        /// <summary>
        /// Soft delete a wishlist item by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var wishlistItem = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(w => w.WishlistId == id, ct);

            if (wishlistItem == null)
                throw new KeyNotFoundException($"Wishlist item with ID {id} not found.");

            wishlistItem.Delete();
            await _dbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Get all wishlist items for a specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<List<WishlistItem>> GetItemsByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _dbContext.WishlistItems
                .Where(w => w.UserId == userId && w.DeletedAt == null)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        /// <summary>
        /// Get all wishlist items
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<List<WishlistItem>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbContext.WishlistItems
                .Where(w => w.DeletedAt == null)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        /// <summary>
        /// Get paged wishlist items with optional filtering and sorting
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortBy"></param>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<(IReadOnlyList<WishlistItem>, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            int? userId = null,
            int? productId = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.WishlistItems
                .Where(w => w.DeletedAt == null)
                .AsQueryable();

            // ====== FILTERS ======
            if (userId.HasValue)
                query = query.Where(w => w.UserId == userId.Value);

            if (productId.HasValue)
                query = query.Where(w => w.ProductId == productId.Value);

            // ====== SORTING ======
            var totalCount = await query.CountAsync(ct);

            if (sortBy?.ToLower() == "oldest")
            {
                query = query.OrderBy(w => w.CreatedAt);
            }
            else if (sortBy?.ToLower() == "newest")
            {
                query = query.OrderByDescending(w => w.CreatedAt);
            }
            else
            {
                // Default sorting by Newest
                query = query.OrderByDescending(w => w.CreatedAt);
            }

            // ====== PAGINATION ======
            var wishlists = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return (wishlists, totalCount);
        }

        /// <summary>
        /// Get count of wishlist items with optional filters
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<int> GetWishlistCountAsync(
            int? userId = null,
            int? productId = null,
            CancellationToken ct = default)
        {
            var query = _dbContext.WishlistItems
                .Where(w => w.DeletedAt == null)
                .AsQueryable();
            if (userId.HasValue)
                query = query.Where(w => w.UserId == userId.Value);
            if (productId.HasValue)
                query = query.Where(w => w.ProductId == productId.Value);
            return await query.CountAsync(ct);
        }
    }
}
