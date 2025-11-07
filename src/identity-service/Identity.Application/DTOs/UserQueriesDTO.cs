using Identity.Domain.Enums;

namespace Identity.Application.DTOs
{
    public class UserQueriesDTO
    {
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? UserAddress { get; set; }
        public DateTime? UserBirthday { get; set; }
        public string? ContactPhone { get; set; }
        public string? Avatar { get; set; }
        public string? CitizenIdCard { get; set; }
        public UserStatus? UserStatus { get; set; }
        public ProfileVerificationStatus? ProfileStatus { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
