using Microsoft.Extensions.Options;
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

            var noti = Notification.Create(
                data.TargetUserId,
                "Khiếu nại mới",
                $"Bạn bị khiếu nại bởi người dùng {data.FromUserId}",
                "ComplaintService",
                ""
            );

            await repo.AddAsync(noti, ct);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public record ComplaintNotificationEvent(int TargetUserId, int FromUserId, string Reason);
}
