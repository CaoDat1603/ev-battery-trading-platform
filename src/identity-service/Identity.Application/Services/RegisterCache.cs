using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain.Abtractions;


namespace Identity.Application.Services
{
    public class RegisterCache : IRegisterCache
    {
        private readonly ICacheService _cacheService;
        public RegisterCache(ICacheService cacheService) 
        {
            _cacheService = cacheService;
        }
        public void Save(RegisterRequest request, string otp, TimeSpan? ttl = null)
        {
            _cacheService.Set(request.Email ?? request.PhoneNumber!, (request, otp), ttl ?? TimeSpan.FromMinutes(15));
        }

        public (RegisterRequest Request, string Otp)? Get(string emailOrPhone)
        {
            return _cacheService.Get<(RegisterRequest,string)>(emailOrPhone);
        }
        public void Remove(string emailOrPhone)
        {
            _cacheService.Remove(emailOrPhone);
        }
    }
}