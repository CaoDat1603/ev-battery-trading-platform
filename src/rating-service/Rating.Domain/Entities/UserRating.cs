using System.ComponentModel.DataAnnotations;

namespace Rating.Domain.Entities
{
    public class UserRating
    {
        [Key]
        public int RateId { get; set; }
        public int? FeedBackId { get; set; } //phản hồi cho phản hồi
        public int UserId { get; set; } // bị đánh giá
        public int RateBy { get; set; } // người dánh giá
        public int? Score { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset? CreateAt { get; set; }
        public DateTimeOffset? UpdateAt { get; set; }
        public DateTimeOffset? DeleteAt { get; set; }
        private UserRating() { }
        public static UserRating Create(int? feedBackId, int userId, int rateBy, int? score, string comment)
        {
            if (userId <= 0) throw new ArgumentException("Invalid userId");
            if (rateBy <= 0) throw new ArgumentException("Invalid rateBy");
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentNullException(nameof(comment), "Comment is required.");
            return new UserRating
            {
                FeedBackId = feedBackId,
                UserId = userId,
                RateBy = rateBy,
                Score = score,
                Comment = comment,
                CreateAt = DateTimeOffset.UtcNow,
            };
        }
        public void Update(int? newScore, string? newComment)
        {
            if (newScore != null)
                Score = newScore;

            if (!string.IsNullOrWhiteSpace(newComment))
                Comment = newComment;

            UpdateAt = DateTimeOffset.UtcNow;
        }
        public void Delete()
        {
            DeleteAt = DateTimeOffset.UtcNow;
        }
    }
}
