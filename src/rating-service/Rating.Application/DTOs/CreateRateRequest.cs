using System.ComponentModel.DataAnnotations;

namespace Rating.Application.DTOs
{
    public class CreateRateRequest
    {
        public int? FeedbackId { get; set; }
        public int? UserId { get; set; }
        public int? ProductId { get; set; }

        [Required]
        public int RateBy { get; set; }

        [Range(1, 10)]
        public int? Score { get; set; }

        [Required, MaxLength(2000)]
        public string Comment { get; set; } = null!;
    }
}
