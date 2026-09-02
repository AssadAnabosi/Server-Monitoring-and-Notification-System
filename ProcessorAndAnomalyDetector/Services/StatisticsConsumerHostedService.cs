using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcessorAndAnomalyDetector.Models;
using RabbitMQClientLibrary;
using RabbitMQClientLibrary.Interfaces;

namespace ProcessorAndAnomalyDetector.Services;

public sealed class StatisticsConsumerHostedService : BackgroundService
{
    private const string ExchangeName = "statistics-exchange";
    private const string QueueName = "statistics-collector-queue";
    private const string BindingPattern = "ServerStatistics.*";

    private readonly IOptions<RabbitMQOptions> _rabbitMqOptions;
    private readonly AnomalyDetectionService _handler;
    private readonly ILogger<StatisticsConsumerHostedService> _logger;

    private IMessageConsumer? _consumer;

    public StatisticsConsumerHostedService(
        IOptions<RabbitMQOptions> rabbitMqOptions,
        AnomalyDetectionService handler,
        ILogger<StatisticsConsumerHostedService> logger)
    {
        _rabbitMqOptions = rabbitMqOptions;
        _handler = handler;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer = await RabbitMQConsumer.CreateAsync(_rabbitMqOptions.Value);

        var consumerTag = await _consumer.ConsumeAsync<ServerStatistics>(
            exchange: ExchangeName,
            queueName: QueueName,
            bindingPattern: BindingPattern,
            handler: _handler.HandleAsync,
            cancellationToken: cancellationToken,
            durable: true,
            prefetchCount: 10,
            autoAck: false);

        _logger.LogInformation(
            "Consumer started with tag: {ConsumerTag} on exchange '{Exchange}', queue '{Queue}'",
            consumerTag, ExchangeName, QueueName);

        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Delay(Timeout.Infinite, stoppingToken)
            .ContinueWith(_ => { }, TaskScheduler.Default);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping consumer...");

        if (_consumer is not null)
        {
            await _consumer.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}