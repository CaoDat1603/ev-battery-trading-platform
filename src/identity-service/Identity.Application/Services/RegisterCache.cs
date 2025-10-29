using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Identity.Application.Services
{
    public class RegisterCache : IRegisterCache
    {
        private readonly IMemoryCache _memoryCache;
        public RegisterCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public void Save(RegisterRequest request, string otp, TimeSpan? ttl = null)
        {
            _memoryCache.Set(request.Email ?? request.PhoneNumber!, (request, otp), ttl ?? TimeSpan.FromMinutes(15));
        }

        public (RegisterRequest Request, string Otp)? Get(string emailOrPhone)
        {
            return _memoryCache.TryGetValue(emailOrPhone, out (RegisterRequest, string) value) ? value : null; ;
        }
        public void Remove(string emailOrPhone)
        {
            _memoryCache.Remove(emailOrPhone);
        }
    }
}