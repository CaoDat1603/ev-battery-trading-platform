namespace Rating.Application.DTOs
{
    public class UserInfoDto
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
        public string? UserStatus { get; set; }
        public string? ProfileStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
