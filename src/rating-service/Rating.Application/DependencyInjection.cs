using Rating.Application.Contracts;
using Rating.Application.Services;

namespace Rating.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRatingApplication(this IServiceCollection services)
        {
            // Services (Application layer)
            services.AddScoped<IRateCommands, RateCommands>();
            services.AddScoped<IRateQueries, RateQueries>();

            return services;
        }
    }
}
