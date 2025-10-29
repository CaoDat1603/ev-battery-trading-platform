using Microsoft.EntityFrameworkCore;
using Rating.Domain.Abstractions;
using Rating.Infrastructure.Repositories;
using Rating.Infrastructure.Services;

namespace Rating.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRatingInfrastructure(this IServiceCollection services, string? connectionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ILocalFileStorage, LocalFileStorage>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<IRateImageHandler, RateImageHandler>();
            return services;
        }
    }
}
