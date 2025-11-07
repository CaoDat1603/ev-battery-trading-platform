using Microsoft.Extensions.Options;
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

            var noti = Notification.Create(
                data.UserId,
                "Đánh giá mới",
                $"Bạn nhận được {data.Score} sao từ {data.FromUserName}",
                "RatingService",
                ""
            );

            await repo.AddAsync(noti, ct);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public record RatingNotificationEvent(int UserId, string FromUserName, double Score);
}
