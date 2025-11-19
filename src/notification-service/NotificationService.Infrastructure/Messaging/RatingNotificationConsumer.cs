using Microsoft.Extensions.Options;
using NotificationService.Application.Contracts;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure.Messaging
{
    public class RatingNotificationConsumer : RabbitMQBaseConsumer<RatingNotificationEvent>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RatingNotificationConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMQSettings> options)
            : base(scopeFactory, options, "rating_exchange", "notification_rating_queue", "RatingService")
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleMessageAsync(RatingNotificationEvent data, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();

            var noti = Notification.Create(
                data.UserId,
                "Đánh giá mới",
                $"Bạn nhận được {data.Score} sao từ {data.FromUserName}",
                "RatingService",
                ""
            );

            await repo.AddAsync(noti, ct);
            await unitOfWork.SaveChangesAsync();

            // Bắn SignalR realtime
            await notifier.SendToUserAsync(
                data.UserId,
                "Đánh giá mới",
                $"Bạn bị đánh giá bởi người dùng {data.FromUserName}",
                "", // link nếu có
                ct
            );
        }
    }

    public record RatingNotificationEvent(int UserId, string FromUserName, double Score);
}
