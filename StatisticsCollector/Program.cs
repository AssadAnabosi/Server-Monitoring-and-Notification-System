using Microsoft.Extensions.Configuration;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;
using RabbitMQClientLibrary;
using RabbitMQClientLibrary.Interfaces;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables() // Overrides appsettings.json
    .Build();

var serverStatisticsConfig = configuration
    .GetSection(ServerStatisticsConfig.SectionName)
    .Get<ServerStatisticsConfig>() ?? throw new InvalidOperationException(
    $"Missing configuration section '{ServerStatisticsConfig.SectionName}'.");

if (serverStatisticsConfig.SamplingIntervalSeconds <= 0)
{
    throw new InvalidOperationException(
        $"{nameof(ServerStatisticsConfig.SamplingIntervalSeconds)} must be greater than zero.");
}

Console.WriteLine(serverStatisticsConfig);

var rabbitMQOptions = configuration
    .GetSection(RabbitMQOptions.SectionName)
    .Get<RabbitMQOptions>() ?? new RabbitMQOptions();
Console.WriteLine(rabbitMQOptions);

var statisticsCollectorStrategy = StatisticsCollectorFactory.CreateCollector();
var statisticsCollectorService = new StatisticsCollectorService(statisticsCollectorStrategy);

await using IMessagePublisher publisher = await RabbitMQPublisher.CreateAsync(rabbitMQOptions);

var exchangeName = "statistics-exchange";
var queueName = "statistics-collector-queue";
var bindingPattern = "ServerStatistics.*";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var timer = new PeriodicTimer(TimeSpan.FromSeconds(serverStatisticsConfig.SamplingIntervalSeconds));
try
{
    do
    {
        await PublishOnce();
    } while (await timer.WaitForNextTickAsync(cts.Token));
}
catch (OperationCanceledException)
{
    Console.WriteLine("Shutdown requested, exiting cleanly.");
}

async Task PublishOnce()
{
    try
    {
        var serverStatistics = statisticsCollectorService.Collect(serverStatisticsConfig.ServerIdentifier);

        Console.WriteLine(serverStatistics);

        await publisher.PublishAsync(
            exchange: exchangeName,
            queueName: queueName,
            routingKey: $"ServerStatistics.{serverStatisticsConfig.ServerIdentifier}",
            bindingPattern: bindingPattern,
            message: serverStatistics,
            cancellationToken: cts.Token);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        Console.Error.WriteLine($"[{DateTime.UtcNow:O}] Publish failed: {ex.Message}");
    }
}