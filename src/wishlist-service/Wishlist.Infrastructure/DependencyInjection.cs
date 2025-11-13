using Microsoft.EntityFrameworkCore;
using Wishlist.Domain.Abstractions;
using Wishlist.Infrastructure.Repositories;

namespace Wishlist.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWishlistInfrastructure(this IServiceCollection services, string? connectionString)
        {

            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
