using Auction.Domain.Abstractions;
using Auction.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuctionInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
            services.AddScoped<IAuctionRepository, AuctionRepository>();
            services.AddScoped<IBidRepository, BidRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
