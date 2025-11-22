
using Microsoft.Extensions.Options;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Messaging
{
    public class OrderNotificationConsumer : RabbitMQBaseConsumer<OrderNotificationEvent>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderNotificationConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMQSettings> options)
            : base(scopeFactory, options, "order_exchange", "notification_order_queue", "OrderService")
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task HandleMessageAsync(OrderNotificationEvent data, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();

            var noti = Notification.Create(
                data.userId,
                data.title,
                $"Mã đơn hàng {data.transactionId}, {data.content}",
                "OrderService",
                ""
            );

            await repo.AddAsync(noti, ct);
            await unitOfWork.SaveChangesAsync();

            // Bắn SignalR realtime
            await notifier.SendToUserAsync(
                data.userId,
                data.title,
                $"Mã đơn hàng {data.transactionId}, {data.content}",
                "",
                ct
            );
        }
    }
    public record OrderNotificationEvent(int userId, int transactionId, string title, string content);
}
