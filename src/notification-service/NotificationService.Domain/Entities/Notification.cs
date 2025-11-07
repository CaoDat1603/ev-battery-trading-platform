using System.Runtime.InteropServices;

namespace NotificationService.Domain.Entities
{
    public class Notification
    {
        public Guid NotificationId { get; set; } = Guid.NewGuid();
        public int UserId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Source { get; set; } = default!; // ví dụ: "RatingService"
        public string? Url { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsRead { get; set; } = false;
        
        private Notification() { }
        public static Notification Create (int userId, string title, string message, string source, string? url)
        {
            if (userId < 0) throw new ArgumentNullException("UserId is null");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("Title is null");
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException("Message is null");
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("Source is null");

            return new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Source = source,
                Url = url ?? null,
                CreatedAt = DateTimeOffset.UtcNow,
                IsRead = false
            };
        }
        public void Update (bool isRead)
        {
            IsRead = isRead;
        }
    }
}
