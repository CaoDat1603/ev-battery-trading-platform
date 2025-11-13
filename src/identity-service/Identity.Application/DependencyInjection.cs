using Identity.Application.Contracts;
using Identity.Application.Services;
using Identity.Domain.Abtractions;

namespace Identity.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityApplication (this IServiceCollection services)
        {
            services.AddScoped<IUserQueries, UserQueries>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRegisterCache, RegisterCache>();
            services.AddScoped<ISystemTokenService, SystemTokenService>();
            services.AddScoped<IUserCommands, UserCommands>();
            return services;
        }
    }
}
