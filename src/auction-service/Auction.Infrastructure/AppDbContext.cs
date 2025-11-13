using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace Auction.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AuctionItem> Auctions => Set<AuctionItem>();
        public DbSet<Bid> Bids => Set<Bid>();

        protected override void OnModelCreating(ModelBuilder m)
        {
            base.OnModelCreating(m);

            m.Entity<AuctionItem>(e =>
            {
                e.HasKey(a => a.AuctionId);
                e.HasMany(a => a.Bids).WithOne().HasForeignKey(b => b.AuctionId);
                e.HasQueryFilter(a => a.DeletedAt == null);
            });

            m.Entity<Bid>(e =>
            {
                e.HasKey(b => b.BidId);
                e.Property(b => b.BidAmount).HasColumnType("decimal(18,2)");
                e.HasQueryFilter(b => b.StatusDeposit != null);
            });

            // Snake_case convention
            foreach (var entity in m.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                entity.SetTableName(ToSnakeCase(tableName!));

                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, null));
                    property.SetColumnName(ToSnakeCase(columnName!));
                }

                foreach (var key in entity.GetKeys()) key.SetName(ToSnakeCase(key.GetName()!));
                foreach (var index in entity.GetIndexes()) index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}
