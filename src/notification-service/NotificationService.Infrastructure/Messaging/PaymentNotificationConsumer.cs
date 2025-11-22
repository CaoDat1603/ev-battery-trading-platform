using Microsoft.Extensions.Options;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Messaging
{
    public class PaymentNotificationConsumer : RabbitMQBaseConsumer<PaymentNotificationEvent>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public PaymentNotificationConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMQSettings> options)
           : base(scopeFactory, options, "payment_exchange", "notification_payment_queue", "PaymentService")
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task HandleMessageAsync(PaymentNotificationEvent data, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();

            var noti = Notification.Create(
                data.userId,
                data.title,
                data.content,
                "PaymentService",
                ""
            );

            await repo.AddAsync(noti, ct);
            await unitOfWork.SaveChangesAsync();

            // Bắn SignalR realtime
            await notifier.SendToUserAsync(
                data.userId,
                data.title,
                data.content,
                "",
                ct
            );
        }
    }
    public record PaymentNotificationEvent(int userId, string title, string content);
}
