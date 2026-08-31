namespace RabbitMQClientLibrary.Interfaces;

public interface IMessagePublisher : IAsyncDisposable
{
    Task PublishAsync<T>(string exchange, string queueName, string routingKey, string bindingPattern, T message, bool durable = true);
}