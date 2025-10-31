
using Microsoft.Extensions.DependencyInjection;
namespace Auction.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuctionApplication(this IServiceCollection services)
        {
            // Register application services here
            return services;
        }
    }
}
