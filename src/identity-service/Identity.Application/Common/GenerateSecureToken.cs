using System.Security.Cryptography;

namespace Identity.Application.Common
{
    public static class GenerateSecureToken
    {
        public static string _GenerateSecureToken(int length = 64)
        {
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
