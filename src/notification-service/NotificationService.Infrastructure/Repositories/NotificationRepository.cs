using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Abstractions;
using NotificationService.Domain.Entities;
using System.Reflection.Metadata.Ecma335;

namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;
        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Notification notification, CancellationToken ct)
            => await _db.Notifications.AddAsync(notification, ct);


        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, CancellationToken ct)
        {
            return await _db.Notifications
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(ct);
        }

        public async Task MarkAsReadAsync(Guid notificationId, CancellationToken ct)
        {
            var noti = await _db.Notifications.FindAsync(notificationId);
            if (noti is null) return;
            noti.IsRead = true;
        }
    }
}
