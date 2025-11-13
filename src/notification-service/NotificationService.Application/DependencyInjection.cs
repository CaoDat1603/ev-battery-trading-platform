using NotificationService.Application.Contracts;
using NotificationService.Application.Services;

namespace NotificationService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationApplication (this IServiceCollection services)
        {
            services.AddScoped<INotificationAppService, NotificationAppService>();
            return services;
        }
    }
}
