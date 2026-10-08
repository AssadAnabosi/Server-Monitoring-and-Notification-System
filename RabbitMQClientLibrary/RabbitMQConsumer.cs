using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQClientLibrary.Interfaces;

namespace RabbitMQClientLibrary;

public class RabbitMQConsumer : RabbitMQClientBase, IMessageConsumer
{
    private RabbitMQConsumer(IConnection connection, IChannel channel)
        : base(connection, channel)
    {
    }

    public static async Task<RabbitMQConsumer> CreateAsync(RabbitMQOptions options)
    {
        var (connection, channel) = await OpenConnectionAsync(options);
        return new RabbitMQConsumer(connection, channel);
    }

    public async Task<string> ConsumeAsync<T>(
        string exchange,
        string queueName,
        string bindingPattern,
        Func<T, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default,
        bool durable = true,
        ushort prefetchCount = 10,
        bool autoAck = false)
    {
        await DeclareTopologyAsync(exchange, queueName, bindingPattern, durable, cancellationToken);

        // Limit unacked messages delivered to this consumer at once
        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: prefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.Span);
                var message = JsonSerializer.Deserialize<T>(json);

                if (message is not null)
                {
                    await handler(message, cancellationToken);
                }
                else
                {
                    Console.WriteLine(
                        $"Deserialized message was null for queue '{queueName}', " +
                        $"routing key '{eventArgs.RoutingKey}', DeliveryTag {eventArgs.DeliveryTag}. " +
                        "Message will be acked and dropped.");
                }

                if (!autoAck)
                {
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false,
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to process message from queue '{queueName}', " +
                    $"routing key '{eventArgs.RoutingKey}'. DeliveryTag: {eventArgs.DeliveryTag}, " +
                    $"ConsumerTag: {eventArgs.ConsumerTag}, Redelivered: {eventArgs.Redelivered}{Environment.NewLine}" +
                    $"{ex}");

                if (!autoAck)
                {
                    // requeue: false sends it to a DLX if configured, or drops it
                    await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false,
                        cancellationToken: cancellationToken);
                }
            }
        };

        var consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: autoAck,
            consumer: consumer,
            cancellationToken: cancellationToken);

        return consumerTag;
    }

    public async Task CancelAsync(string consumerTag)
    {
        await _channel.BasicCancelAsync(consumerTag);
    }
}