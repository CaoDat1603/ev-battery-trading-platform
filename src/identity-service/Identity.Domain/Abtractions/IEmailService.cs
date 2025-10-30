namespace Identity.Domain.Abtractions
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
