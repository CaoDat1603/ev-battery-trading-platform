using Catalog.Domain.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for managing Product entities.
    /// Handles CRUD operations, soft deletion, and advanced search/filter logic.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db) => _db = db;

        /// <summary>
        /// Adds a new product entity to the database context.
        /// </summary>
        public async Task AddAsync(Product entity, CancellationToken ct = default)
            => await _db.Products.AddAsync(entity, ct).AsTask();

        /// <summary>
        /// Retrieves a product by its ID, including its associated details.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the product cannot be found.</exception>
        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var product = await _db.Products
                .Include(p => p.Details)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id, ct);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            return product;
        }

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        public Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _db.Products.Update(product);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs a soft delete on the specified product by marking it as deleted.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the product does not exist.</exception>
        public async Task SoftDeleteAsync(int productId, CancellationToken ct = default)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.DeletedAt == null, ct);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID = {productId} not found.");

            product.Delete();
            _db.Products.Update(product);
        }

        /// <summary>
        /// Searches products by their product ID (includes product details).
        /// </summary>
        public async Task<IReadOnlyList<Product>> SearchByProductIDAsync(int id, CancellationToken ct = default)
        {
            return await _db.Products
                .Where(p => p.ProductId == id && p.DeletedAt == null)
                .Include(p => p.Details)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Retrieves all products that have not been deleted.
        /// </summary>
        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Products
                .Include(p => p.Details)
                .Where(p => p.DeletedAt == null)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Retrieves all products belonging to a specific seller.
        /// </summary>
        public async Task<IReadOnlyList<Product>> SearchBySellerAsync(int sellerId, CancellationToken ct = default)
        {
            return await _db.Products
                .Where(p => p.SellerId == sellerId && p.DeletedAt == null)
                .Include(p => p.Details)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Retrieves all products belonging to a specific moderated by.
        /// </summary>
        public async Task<IReadOnlyList<Product>> SearchModeratedByAsync(int id, CancellationToken ct = default)
        {
            return await _db.Products
                .Where(p => p.ModeratedBy == id && p.DeletedAt == null)
                .Include(p => p.Details)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Retrieves paged products with multiple filtering options such as keyword, price range, address, and status.
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <param name="keyword">Keyword for title search (case-insensitive).</param>
        /// <param name="minPrice">Minimum price filter.</param>
        /// <param name="maxPrice">Maximum price filter.</param>
        /// <param name="pickupAddress">Address filter (supports partial match).</param>
        /// <param name="status">Product status filter.</param>
        /// <param name="sellerId">Seller ID filter.</param>
        /// <returns>Tuple containing the filtered products and total record count.</returns>
        public async Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? keyword = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? pickupAddress = null,
            ProductStatus? status = null,
            int? sellerId = null,
            CancellationToken ct = default)
        {
            var query = _db.Products
                .Include(p => p.Details)
                .Where(p => p.DeletedAt == null)
                .AsQueryable();

            // ====== FILTERS ======

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => EF.Functions.ILike(p.Title, $"%{keyword}%"));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Filter by pickup address (supports province or province + district)
            if (!string.IsNullOrWhiteSpace(pickupAddress))
            {
                var addressParts = pickupAddress
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(a => a.ToLower())
                    .ToArray();

                // Example: "Hanoi"
                if (addressParts.Length == 1)
                {
                    var province = addressParts[0];
                    query = query.Where(p =>
                        EF.Functions.ILike(p.PickupAddress.ToLower(), $"%{province}%"));
                }
                // Example: "Hanoi, Ba Dinh"
                else
                {
                    foreach (var part in addressParts)
                    {
                        var localPart = part;
                        query = query.Where(p =>
                            EF.Functions.ILike(p.PickupAddress.ToLower(), $"%{localPart}%"));
                    }
                }
            }

            if (status.HasValue)
                query = query.Where(p => p.StatusProduct == status.Value);

            if (sellerId.HasValue)
                query = query.Where(p => p.SellerId == sellerId.Value);

            // ====== PAGINATION ======
            var totalCount = await query.CountAsync(ct);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return (products, totalCount);
        }
    }
}
