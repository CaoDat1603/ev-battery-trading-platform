namespace NotificationService.Infrastructure.Settings
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "rabbitmq";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "ev_user";
        public string Password { get; set; } = "ev_pass_very_secret";

        public RabbitMQExchange Exchange { get; set; } = new();
        public RabbitMQQueues Queues { get; set; } = new();
    }

    public class RabbitMQExchange
    {
        public string Rating { get; set; } = "rating_exchange";
        public string Complaint { get; set; } = "complaint_exchange";
        public string Identity { get; set; } = "identity_exchange";
    }

    public class RabbitMQQueues
    {
        public string Rating { get; set; } = "notification_rating_queue";
        public string Complaint { get; set; } = "notification_complaint_queue";
        public string Identity { get; set; } = "notification_identity_queue";
    }

}
