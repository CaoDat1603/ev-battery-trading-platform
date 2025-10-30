using Identity.Application.DTOs;
using Identity.Application.Services;

namespace Identity.Application.Contracts
{
    public interface IRegisterCache
    {
        void Save(RegisterRequest request, string otp, TimeSpan? expiry = null);
        (RegisterRequest Request, string Otp)? Get(string emailOrPhone);
        void Remove(string emailOrPhone);
    }
   
}
