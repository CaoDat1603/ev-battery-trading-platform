using Microsoft.Extensions.Configuration;
namespace Identity.Infrastructure.Services
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
