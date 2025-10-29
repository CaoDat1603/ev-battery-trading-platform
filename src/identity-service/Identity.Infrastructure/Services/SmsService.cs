using Identity.Domain.Abtractions;

namespace Identity.Infrastructure.Services
{
    public class SmsService: ISmsService
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            Console.WriteLine($"Send SMS to {phoneNumber}: {message}");
            return Task.CompletedTask;
        }
    }

}
