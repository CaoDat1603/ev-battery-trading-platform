using Complaints.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace Complaints.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Complaint> Complaints => Set<Complaint>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);
            m.Entity<Complaint>().HasKey(c => c.ComplaintId);
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
