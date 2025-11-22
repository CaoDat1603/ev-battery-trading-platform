using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Abstractions;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Settings;
using NotificationService.Infrastructure.SignalR;

namespace NotificationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration, string? connectionString)
        {
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddSignalR();
            services.AddScoped<IRealtimeNotifier, SignalRNotifier>();

            services.AddHostedService<RatingNotificationConsumer>();
            services.AddHostedService<ComplaintNotificationConsumer>();
            services.AddHostedService<OrderNotificationConsumer>();
            services.AddHostedService<PaymentNotificationConsumer>();

            services.Configure<RabbitMQSettings>(
                configuration.GetSection("RabbitMQ"));


            return services;
        }
    }
}
