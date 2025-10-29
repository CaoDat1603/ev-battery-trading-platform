namespace Rating.Application.DTOs
{
    public class RateImageDto
    {
        public int RateImageId { get; set; }
        public string ImageUrl { get; set; } = default!;
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
