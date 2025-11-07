using Identity.Application.Contracts;
using Identity.Application.Services;
using Identity.Domain.Abtractions;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, string? connectionString, IConfiguration configuration)
        {
            services.AddScoped<ISystemJwtProvider, SystemJwtProvider>();
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFileStorage, LocalFileStorage>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IUserQueries, UserQueries>();
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            return services;
        }
    }
}
