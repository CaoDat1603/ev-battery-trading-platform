using Wishlist.Application.Contracts;
using Wishlist.Application.Services;

namespace Wishlist.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWishlistApplication(this IServiceCollection services)
        {
            services.AddScoped<IWishlistCommands, WishlistCommands>();
            services.AddScoped<IWishlistQueries, WishlistQueries>();

            return services;
        }
    }
}
