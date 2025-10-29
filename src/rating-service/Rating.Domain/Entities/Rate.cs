using System;
using System.ComponentModel.DataAnnotations;

namespace Rating.Domain.Entities
{
    public class Rate
    {
        [Key]
        public int RateId { get; set; }

        public int? FeedBackId { get; set; }
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public int RateBy { get; set; }

        public int? Score { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public List<RateImage> Images { get; set; } = new List<RateImage>();

        private Rate() { }

        public static Rate Create(int? feedBackId, int? userId, int? productId, int rateBy, int? score, string comment)
        {
            // đúng 1 mục tiêu
            if (userId == null == (productId == null))
                throw new ArgumentException("Must rate either a user or a product, not both.");

            if (rateBy <= 0) throw new ArgumentException(nameof(rateBy));
            if (userId != null && userId == rateBy) throw new InvalidOperationException("Cannot rate yourself.");
            if (string.IsNullOrWhiteSpace(comment)) throw new ArgumentNullException(nameof(comment));
            if (score <= 0 || score > 10) throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 10.");

            return new Rate
            {
                FeedBackId = feedBackId,
                UserId = userId,
                ProductId = productId,
                RateBy = rateBy,
                Score = score,
                Comment = comment.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }

        public void Update(int? newScore = 0, string? newComment = null)
        {
            bool change = false;
            if (newScore <= 0 || newScore > 10) throw new ArgumentOutOfRangeException(nameof(newScore), "Score must be between 1 and 10.");
            if (Score != newScore) { Score = newScore; change = true; }
            if (!string.IsNullOrWhiteSpace(newComment))
            {
                var c = newComment.Trim();
                if (c != Comment) { Comment = c; change = true; }
            }
            if (change) UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Delete()
        {
            DeletedAt = DateTimeOffset.UtcNow;
        }
        public RateImage AddImage(string imageUrl)
        {
            var img = RateImage.Create(RateId, imageUrl);
            Images.Add(img);
            return img;
        }
        public void UpdateImage(int rateImageId, string newUrl)
        {
            var img = Images.FirstOrDefault(i => i.RateImageId == rateImageId);
            if (img == null) throw new KeyNotFoundException("Rate image not found.");
            img.UpdateUrl(newUrl);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        public void RemoveImage(int rateImageId)
        {
            var img = Images.FirstOrDefault(i => i.RateImageId == rateImageId);
            if (img == null) return;
            Images.Remove(img);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Thay thế toàn bộ danh sách ảnh bằng list URL mới
        public void ReplaceImages(IEnumerable<string> newUrls)
        {
            Images.Clear();
            foreach (var url in newUrls)
                Images.Add(RateImage.Create(RateId, url));
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}