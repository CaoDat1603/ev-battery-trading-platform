namespace Identity.Application.DTOs
{
    public class ResendOtpRequest
    {
        public string EmailOrPhone { get; set; } = string.Empty;
    }
}
