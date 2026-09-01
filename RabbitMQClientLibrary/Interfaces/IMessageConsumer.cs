namespace RabbitMQClientLibrary.Interfaces;

public interface IMessageConsumer : IAsyncDisposable
{
    Task<string> ConsumeAsync<T>(
        string exchange,
        string queueName,
        string bindingPattern,
        Func<T, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default,
        bool durable = true,
        ushort prefetchCount = 10,
        bool autoAck = false);

    Task CancelAsync(string consumerTag);
}