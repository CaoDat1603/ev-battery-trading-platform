namespace Rating.Application.DTOs
{
    public class RateResponse
    {
        public int RateId { get; set; }
        public int? FeedbackId { get; set; }
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public int RateBy { get; set; }
        public int? Score { get; set; }
        public string Comment { get; set; } = default!;
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public IReadOnlyList<RateImageDto> Images { get; set; } = Array.Empty<RateImageDto>();
    }
}
