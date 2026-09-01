using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQClientLibrary.Interfaces;

namespace RabbitMQClientLibrary;

public class RabbitMQPublisher : RabbitMQClientBase, IMessagePublisher
{
    private RabbitMQPublisher(IConnection connection, IChannel channel)
        : base(connection, channel)
    {
    }

    public static async Task<RabbitMQPublisher> CreateAsync(RabbitMQOptions options)
    {
        var (connection, channel) = await OpenConnectionAsync(options);
        return new RabbitMQPublisher(connection, channel);
    }

    public async Task PublishAsync<T>(
        string exchange,
        string queueName,
        string routingKey,
        string bindingPattern,
        T message,
        CancellationToken cancellationToken = default,
        bool durable = true)
    {
        await DeclareTopologyAsync(exchange, queueName, bindingPattern, durable, cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = new BasicProperties { Persistent = durable };

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}