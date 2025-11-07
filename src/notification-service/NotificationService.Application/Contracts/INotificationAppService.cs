using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Contracts
{
    public interface INotificationAppService
    {
        Task CreateAndNotifyAsync(NotificationDto dto, CancellationToken ct);
        Task<IEnumerable<Notification>> GetNotificationByUserId(int userId, CancellationToken ct = default);
        Task ReadNotificationAsync(Guid notificationId, CancellationToken ct = default);
        
    }
}
