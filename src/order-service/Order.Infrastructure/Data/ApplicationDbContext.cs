using Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Order.Domain.Entities.Transaction> Transactions { get; set; }
        public DbSet<Order.Domain.Entities.FeeSettings> FeeSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Khóa chính (Primary Key) cho Bảng FeeSettings
            modelBuilder.Entity<FeeSettings>(entity =>
            {
                // Chỉ định FeeId là Khóa chính
                entity.HasKey(e => e.FeeId);
            });

            // Cấu hình cho Bảng Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
            });
        }
    }
}