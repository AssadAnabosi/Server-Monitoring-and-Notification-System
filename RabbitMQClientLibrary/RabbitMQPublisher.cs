using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQClientLibrary.Interfaces;

namespace RabbitMQClientLibrary;

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMQPublisher(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMQPublisher> CreateAsync(RabbitMQOptions options)
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

        return new RabbitMQPublisher(connection, channel);
    }

    public async Task PublishAsync<T>(
        string exchange,
        string queueName,
        string routingKey,
        string bindingPattern,
        T message,
        bool durable = true)
    {
        // Ensure the topic exchange exists
        await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: durable);

        // Ensure the queue exists
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: durable,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Bind the queue to the exchange using a topic pattern (e.g. "orders.*", "orders.#")
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchange,
            routingKey: bindingPattern);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = new BasicProperties { Persistent = durable };

        // Publish with the actual routing key — this is what gets matched against bindings
        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}