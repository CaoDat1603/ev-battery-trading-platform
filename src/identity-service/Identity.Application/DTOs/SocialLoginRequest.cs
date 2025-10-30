namespace Identity.Application.DTOs
{
    public class SocialLoginRequest
    {
        public string Provider { get; set; } = default!; // "google", "facebook", "apple"
        public string ProviderToken { get; set; } = default!;
    }
}
