using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NotificationService.Domain.Entities;
using System.Text.RegularExpressions;

namespace NotificationService.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext () { }
        public AppDbContext (DbContextOptions<AppDbContext> options) : base (options) { }
        public DbSet<Notification> Notifications => Set<Notification>();
        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);
            m.Entity<Notification>(e =>
            {
                e.HasKey(e => e.NotificationId);
            });
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
