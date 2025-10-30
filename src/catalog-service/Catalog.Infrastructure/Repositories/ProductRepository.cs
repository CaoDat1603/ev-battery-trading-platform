using Catalog.Domain.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db) => _db = db;

        public Task AddAsync(Product entity, CancellationToken ct = default)
            => _db.Products.AddAsync(entity, ct).AsTask();

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Products.Include(p => p.Details).FirstOrDefaultAsync(p => p.ProductId == id, ct);

        public Task<IReadOnlyList<Product>> SearchByProductIDAsync(int id, CancellationToken ct = default)
            => _db.Products
            .Where(p => p.ProductId == id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Product>)t.Result, ct);

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Products
                .Include(p => p.Details)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public Task<IReadOnlyList<Product>> SearchBySellerAsync(int sellerId, CancellationToken ct = default)
            => _db.Products
            .Where(p => p.SellerId == sellerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Product>)t.Result, ct);

        public async Task<IReadOnlyList<Product>> SearchWithFiltersAsync(
            string? keyword,
            decimal? minPrice,
            decimal? maxPrice,
            string? pickupAddress,
            ProductStatus? status,
            int take = 50,
            CancellationToken ct = default)
                {
                    var query = _db.Products.AsQueryable();

                    if (!string.IsNullOrWhiteSpace(keyword))
                        query = query.Where(p => EF.Functions.ILike(p.Title, $"%{keyword}%"));

                    if (minPrice.HasValue)
                        query = query.Where(p => p.Price >= minPrice.Value);

                    if (maxPrice.HasValue)
                        query = query.Where(p => p.Price <= maxPrice.Value);

                    if (!string.IsNullOrWhiteSpace(pickupAddress))
                        query = query.Where(p => EF.Functions.ILike(p.PickupAddress, $"%{pickupAddress}%"));

                    if (status.HasValue)
                        query = query.Where(p => p.StatusProduct == status.Value);

                    return await query
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(take)
                        .AsNoTracking()
                        .ToListAsync(ct);
                }

        public void Update(Product product)
            => _db.Products.Update(product);
    }

}
