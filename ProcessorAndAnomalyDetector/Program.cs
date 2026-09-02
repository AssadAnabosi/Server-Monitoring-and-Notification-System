using Microsoft.Extensions.Configuration;
using ProcessorAndAnomalyDetector.Models;
using ProcessorAndAnomalyDetector.Repositories;
using ProcessorAndAnomalyDetector.Services;
using RabbitMQClientLibrary;
using RabbitMQClientLibrary.Interfaces;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var rabbitMQOptions = configuration
    .GetSection(RabbitMQOptions.SectionName)
    .Get<RabbitMQOptions>() ?? new RabbitMQOptions();

var mongoOptions = configuration
    .GetSection(MongoDbOptions.SectionName)
    .Get<MongoDbOptions>() ?? new MongoDbOptions();

var anomalyDetectionConfig = configuration
    .GetSection(AnomalyDetectionConfig.SectionName)
    .Get<AnomalyDetectionConfig>() ?? throw new InvalidOperationException(
    $"Missing configuration section '{AnomalyDetectionConfig.SectionName}'.");

Console.WriteLine(anomalyDetectionConfig);

IServerStatisticsRepository repository = new ServerStatisticsRepository(mongoOptions);
var service = new ServerStatisticsService(repository);
var handler = new AnomalyDetectionService(anomalyDetectionConfig, service);

var exchangeName = "statistics-exchange";
var queueName = "statistics-collector-queue";
var bindingPattern = "ServerStatistics.*";

await using IMessageConsumer consumer = await RabbitMQConsumer.CreateAsync(rabbitMQOptions);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

Console.WriteLine($"Listening for messages on exchange '{exchangeName}', queue '{queueName}'...");

var consumerTag = await consumer.ConsumeAsync<ServerStatistics>(
    exchange: exchangeName,
    queueName: queueName,
    bindingPattern: bindingPattern,
    handler: handler.HandleAsync,
    cancellationToken: cts.Token,
    durable: true,
    prefetchCount: 10,
    autoAck: false);

Console.WriteLine($"Consumer started with tag: {consumerTag}");

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Stopping consumer...");
}