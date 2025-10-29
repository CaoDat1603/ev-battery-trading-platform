using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace Identity.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);

            // --- User ---
            m.Entity<User>(e =>
            {
                e.HasKey(x => x.UserId);
                e.Property(x => x.UserEmail).HasMaxLength(50).IsRequired(false);
                e.Property(x => x.UserPhone).HasMaxLength(20).IsRequired(false);
                e.Property(x => x.UserPassword).HasMaxLength(256).IsRequired();

                // Quan hệ 1-1
                e.HasOne(x => x.UserProfile)
                    .WithOne(x => x.User)
                    .HasForeignKey<UserProfile>(x => x.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // Soft delete filter
                e.HasQueryFilter(x => x.DeletedAt == null);
            });

            // --- UserProfile ---
            m.Entity<UserProfile>(e =>
            {
                // UserId vừa là PK vừa là FK
                e.HasKey(x => x.UserId);

                e.HasOne(x => x.User)
                    .WithOne(x => x.UserProfile)
                    .HasForeignKey<UserProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasQueryFilter(x => x.DeletedAt == null);
            });

            // --- Tự động snake_case ---
            foreach (IMutableEntityType entity in m.Model.GetEntityTypes())
            {
                var table = entity.GetTableName();
                if (table == null) continue;

                entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(ToSnakeCase(
                        property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, null))!
                    ));

                foreach (var key in entity.GetKeys())
                    key.SetName(ToSnakeCase(key.GetName()!));

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
