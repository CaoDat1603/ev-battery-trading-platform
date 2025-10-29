using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Rating.Domain.Entities;
using System.Text.RegularExpressions;

namespace Rating.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Rate> Rates => Set <Rate>();
        public DbSet<RateImage> ImageRatings => Set <RateImage>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);
            m.Entity<Rate>(e => {
                e.HasKey(c => c.RateId);
                e.HasMany(c => c.Images).WithOne( c => c.Rate).HasForeignKey(c => c.RateId).OnDelete(DeleteBehavior.Cascade);
            });
            m.Entity<RateImage>().HasKey(x => x.RateImageId);
            
            foreach (IMutableEntityType entity in m.Model.GetEntityTypes())
            {
                // Tên bảng
                entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

                // Cột
                foreach (var property in entity.GetProperties())
                    property.SetColumnName(ToSnakeCase(property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, null))!));

                // Khóa
                foreach (var key in entity.GetKeys())
                    key.SetName(ToSnakeCase(key.GetName()!));

                // Chỉ mục
                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }
        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}
