namespace NotificationService.Application.DTOs
{
    public record NotificationDto
    {
        public int UserId { get; init; }
        public string Title { get; init; }
        public string Message { get; init; }
        public string Source { get; init; }
        public string Link { get; init; }
    }

}
