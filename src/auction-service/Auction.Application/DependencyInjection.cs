
using Auction.Application.Contracts;
using Auction.Application.Services;
using Microsoft.Extensions.DependencyInjection;
namespace Auction.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuctionApplication(this IServiceCollection services)
        {
            // Register commands
            services.AddScoped<IAuctionCommand, AuctionCommand>();
            services.AddScoped<IBidCommand, BidCommand>();

            // Register queries
            services.AddScoped<IAuctionQueries, AuctionQueries>();
            services.AddScoped<IBidQueries, BidQueries>();
            return services;
        }
    }
}
