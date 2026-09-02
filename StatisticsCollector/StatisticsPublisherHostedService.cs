using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQClientLibrary;
using RabbitMQClientLibrary.Interfaces;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;

namespace StatisticsCollector;

public sealed class StatisticsPublisherHostedService : BackgroundService
{
    private const string ExchangeName = "statistics-exchange";
    private const string QueueName = "statistics-collector-queue";
    private const string BindingPattern = "ServerStatistics.*";

    private readonly IOptions<RabbitMQOptions> _rabbitMqOptions;
    private readonly ServerStatisticsConfig _serverStatisticsConfig;
    private readonly StatisticsCollectorService _statisticsCollectorService;
    private readonly ILogger<StatisticsPublisherHostedService> _logger;

    private IMessagePublisher? _publisher;

    public StatisticsPublisherHostedService(
        IOptions<RabbitMQOptions> rabbitMqOptions,
        IOptions<ServerStatisticsConfig> serverStatisticsConfig,
        StatisticsCollectorService statisticsCollectorService,
        ILogger<StatisticsPublisherHostedService> logger)
    {
        _rabbitMqOptions = rabbitMqOptions;
        _serverStatisticsConfig = serverStatisticsConfig.Value;
        _statisticsCollectorService = statisticsCollectorService;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Config: {Config}", _serverStatisticsConfig);
        _logger.LogInformation("RabbitMQ options: {Options}", _rabbitMqOptions.Value);

        _publisher = await RabbitMQPublisher.CreateAsync(_rabbitMqOptions.Value);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_serverStatisticsConfig.SamplingIntervalSeconds));

        try
        {
            do
            {
                await PublishOnceAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Shutdown requested, exiting cleanly.");
        }
    }

    private async Task PublishOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            var serverStatistics = _statisticsCollectorService.Collect(_serverStatisticsConfig.ServerIdentifier);

            _logger.LogInformation("{Statistics}", serverStatistics);

            await _publisher!.PublishAsync(
                exchange: ExchangeName,
                queueName: QueueName,
                routingKey: $"ServerStatistics.{_serverStatisticsConfig.ServerIdentifier}",
                bindingPattern: BindingPattern,
                message: serverStatistics,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Publish failed: {Message}", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_publisher is not null)
        {
            await _publisher.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}