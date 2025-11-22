namespace Order.Application.Contracts
{
    public interface IEventBus
    {
        Task PublishAsync<T>(string exchangeName, T message);
    }
}
