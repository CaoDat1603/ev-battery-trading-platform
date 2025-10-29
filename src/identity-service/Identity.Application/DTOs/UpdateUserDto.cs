using Identity.Domain.Enums;

namespace Identity.Application.DTOs
{
    public class UpdateUserDto
    {
        public string? UserFullName { get; set; }
        public string? UserAddress { get; set; }
        public DateTime? UserBirthday { get; set; }
        public string? ContactPhone { get; set; }

        // Lưu URL
        public IFormFile? Avatar { get; set; }
        public IFormFile? CitizenIdCard { get; set; }
    }
}
