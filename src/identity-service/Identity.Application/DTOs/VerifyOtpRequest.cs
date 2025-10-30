namespace Identity.Application.DTOs
{
    public class VerifyOtpRequest
    {
        public string EmailOrPhone { get; set; } = default!; // recipient
        public string OtpCode { get; set; } = default!;
    }
}
