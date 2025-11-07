namespace NotificationService.Application.Contracts
{
    public interface IRealtimeNotifier
    {
        Task SendToUserAsync(int userId, string title, string message, string link, CancellationToken ct = default);
    }
}
