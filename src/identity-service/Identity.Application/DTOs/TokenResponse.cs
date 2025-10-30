﻿namespace Identity.Application.DTOs
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
