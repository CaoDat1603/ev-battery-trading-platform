using Microsoft.AspNetCore.Identity;

namespace Identity.Application.DTOs
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? PhoneNumber {  get; set; }
        public string Password { get; set; } = null!;
    }
}
