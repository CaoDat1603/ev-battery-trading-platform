using Identity.Domain.Enums;

namespace Identity.Application.DTOs
{
    public class RegisterRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; } 
        public string? PhoneNumber { get; set; }
        public string Password { get; set; }
    }

}
