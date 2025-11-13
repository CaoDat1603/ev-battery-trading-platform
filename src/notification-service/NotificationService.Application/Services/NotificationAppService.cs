using NotificationService.Application.Contracts;
using NotificationService.Application.DTOs;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using System.ComponentModel;

namespace NotificationService.Application.Services
{
    public class NotificationAppService : INotificationAppService
    {
        private readonly INotificationRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotifier _notifier;

        public NotificationAppService(
            INotificationRepository repo,
            IUnitOfWork unitOfWork,
            IRealtimeNotifier notifier)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
            _notifier = notifier;
        }

        public async Task CreateAndNotifyAsync(NotificationDto dto, CancellationToken ct)
        {
            var entity = Notification.Create(
                dto.UserId,
                dto.Title,
                dto.Message,
                dto.Source,
                dto.Link
            );

            await _repo.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _notifier.SendToUserAsync(dto.UserId, dto.Title, dto.Message, dto.Link, ct);
        }
        public async Task<IEnumerable<Notification>> GetNotificationByUserId(int userId, CancellationToken ct = default)
        {
            return await _repo.GetByUserIdAsync(userId, ct);
        }
        public async Task ReadNotificationAsync (Guid notificationId, CancellationToken ct = default)
        {
            await _repo.MarkAsReadAsync(notificationId, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
