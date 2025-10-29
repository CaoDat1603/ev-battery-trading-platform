using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rating.Domain.Entities
{
    public class RateImage
    {
        [Key]
        public int RateImageId { get; set; }

        [Required]
        public int RateId { get; set; } // FK tới Rate

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } // URL hình ảnh

        public DateTimeOffset? CreatedAt { get; set; }

        [ForeignKey("RateId")]
        public Rate Rate { get; set; } // Navigation property
        private RateImage() { }
        public static RateImage Create(int rateId, string imageUrl)
        {
            if (rateId <= 0) throw new ArgumentException(nameof(rateId));
            if (string.IsNullOrWhiteSpace(imageUrl)) throw new ArgumentNullException(nameof(imageUrl));

            return new RateImage
            {
                RateId = rateId,
                ImageUrl = imageUrl.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        public void UpdateUrl(string newUrl)
        {
            if (string.IsNullOrWhiteSpace(newUrl)) throw new ArgumentNullException(nameof(newUrl));
            var trimmed = newUrl.Trim();
            if (trimmed.Length > 500) throw new ArgumentException("ImageUrl too long (max 500).", nameof(newUrl));
            ImageUrl = trimmed;
        }
    }
}
