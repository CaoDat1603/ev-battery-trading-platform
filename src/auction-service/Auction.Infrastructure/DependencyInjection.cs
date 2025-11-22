using Auction.Domain.Abstractions;
using Auction.Application.Abstractions;
using Auction.Infrastructure.Repositories;
using Auction.Infrastructure.Services;
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
            services.AddHttpClient<IOrderClient, OrderClient>();
            services.AddHttpClient<IInternalTokenService, InternalTokenService>();
            return services;
        }
    }
}
