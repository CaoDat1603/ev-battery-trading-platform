using Identity.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Domain.Entities
{
    public class UserProfile
    {
        [Key, ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        [MaxLength(100)]
        public string? UserFullName { get; set; }
        [MaxLength(200)]
        public string? UserAddress { get; set; }
        public DateTime? UserBirthday { get; set; }
        public string? ContactPhone { get; set; }

        // Lưu URL
        public string? Avatar { get ;set; }
        public string? CitizenIdCard { get;set; }
        [MaxLength(20)]
        public ProfileVerificationStatus Status { get; set; } = ProfileVerificationStatus.Unverified; // "verified" | "unverified"

        public decimal? TotalAmountPurchase { get; set; } = 0.000m;
        public decimal? TotalAmountSold { get; set; } = 0.000m;
        public string? RejectionReason { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        private UserProfile() { }
        public UserProfile ( string fullname, string address, DateTime? birthday, string? avatarUrl, string? citizenIdCard, ProfileVerificationStatus status, string? contactPhone)
        {

            if (string.IsNullOrWhiteSpace(fullname))
                throw new ArgumentException("Fullname không được để trống.", nameof(fullname));

            if (birthday.HasValue && birthday.Value > DateTime.UtcNow)
                throw new ArgumentException("Birthday không được ở tương lai.", nameof(birthday));

            UserFullName = fullname;
            UserAddress = address;
            UserBirthday = birthday;
            Avatar = avatarUrl;
            CitizenIdCard = citizenIdCard;
            ContactPhone = contactPhone;
            Status = status;

            CreatedAt = DateTimeOffset.UtcNow;

        }
        public void SetStatus(ProfileVerificationStatus status)
        {
            Status = status;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        public void Reject(string? reason)
        {
            Status = ProfileVerificationStatus.Rejected;
            RejectionReason = reason;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

    }
}
