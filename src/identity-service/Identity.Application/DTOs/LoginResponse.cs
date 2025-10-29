namespace Identity.Application.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireAt { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? Role { get; set; }
    }
}
