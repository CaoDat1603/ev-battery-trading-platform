using Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Order.Domain.Entities.Transaction> Transactions { get; set; }
    }
}