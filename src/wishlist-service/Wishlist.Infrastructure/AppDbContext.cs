using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);

            // Cấu hình entity WishlistItem
            m.Entity<WishlistItem>(e =>
            {
                e.HasKey(x => x.WishlistId);

                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.ProductId).IsRequired();
                e.HasQueryFilter(x => x.DeletedAt == null);
            });

            // --- Chuyển toàn bộ sang snake_case ---
            foreach (IMutableEntityType entity in m.Model.GetEntityTypes())
            {
                // Tên bảng
                var tableName = entity.GetTableName();
                entity.SetTableName(ToSnakeCase(tableName!));

                // Cột
                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, null));
                    property.SetColumnName(ToSnakeCase(columnName!));
                }

                // Khóa
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(ToSnakeCase(key.GetName()!));
                }

                // Index
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
                }
            }
        }

        // Hàm chuyển PascalCase → snake_case
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}