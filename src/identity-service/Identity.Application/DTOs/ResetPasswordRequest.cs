namespace Identity.Application.DTOs
{
    public class ResetPasswordRequest
    {
        public string TokenOrOtp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
