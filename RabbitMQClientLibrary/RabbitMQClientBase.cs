using RabbitMQ.Client;

namespace RabbitMQClientLibrary;

public abstract class RabbitMQClientBase(IConnection connection, IChannel channel) : IAsyncDisposable
{
    protected readonly IConnection _connection = connection;
    protected readonly IChannel _channel = channel;

    protected static async Task<(IConnection Connection, IChannel Channel)> OpenConnectionAsync(
        RabbitMQOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return (connection, channel);
    }

    protected async Task DeclareTopologyAsync(
        string exchange,
        string queueName,
        string bindingPattern,
        bool durable,
        CancellationToken cancellationToken)
    {
        // Ensure the topic exchange exists
        await _channel.ExchangeDeclareAsync(
            exchange,
            ExchangeType.Topic,
            cancellationToken: cancellationToken,
            durable: durable);

        // Ensure the queue exists
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: durable,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Bind the queue to the exchange using a topic pattern
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchange,
            routingKey: bindingPattern,
            cancellationToken: cancellationToken);
    }


    public virtual async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}