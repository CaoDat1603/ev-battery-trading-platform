using Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Transaction> Transactions { get; set; }
    }
}