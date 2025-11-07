using Microsoft.AspNetCore.SignalR;
using NotificationService.Application.Contracts;

namespace NotificationService.Infrastructure.SignalR
{
    public class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRNotifier(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task SendToUserAsync(int userId, string title, string message, string link, CancellationToken ct = default)
        {
            Console.WriteLine($"[SignalRNotifier] Sending to user {userId}: {title} - {message}");
            await _hub.Clients.Group(userId.ToString())
                .SendAsync("ReceiveNotification", new { title, message, link }, ct);
        }
    }
}
