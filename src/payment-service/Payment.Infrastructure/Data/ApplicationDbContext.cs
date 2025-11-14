using Microsoft.EntityFrameworkCore;

namespace Payment.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){ }
        public DbSet<Payment.Domain.Entities.Payment> Payments { get; set; }
    }
}