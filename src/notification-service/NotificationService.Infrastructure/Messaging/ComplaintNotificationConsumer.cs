using Microsoft.Extensions.Options;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Messaging
{
    public class ComplaintNotificationConsumer
        : RabbitMQBaseConsumer<ComplaintNotificationEvent>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ComplaintNotificationConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMQSettings> options)
            : base(scopeFactory, options, "complaint_exchange", "notification_complaint_queue", "ComplaintService")
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleMessageAsync(ComplaintNotificationEvent data, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();

            var noti = Notification.Create(
                data.userId,
                "Khiếu nại mới",
                data.content,
                "ComplaintService",
                ""
            );

            await repo.AddAsync(noti, ct);
            await unitOfWork.SaveChangesAsync();

            // Bắn SignalR realtime
            await notifier.SendToUserAsync(
                data.userId,
                "Khiếu nại mới",
                data.content,
                "", // link nếu có
                ct
            );
        }
    }

    public record ComplaintNotificationEvent(int userId, string content);
}
