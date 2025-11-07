using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Abstractions
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken ct);
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, CancellationToken ct);
        Task MarkAsReadAsync(Guid id, CancellationToken ct);
    }
}
