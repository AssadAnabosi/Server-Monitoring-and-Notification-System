using Microsoft.Extensions.Configuration;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;
using RabbitMQClientLibrary;
using RabbitMQClientLibrary.Interfaces;

// TODO: DI Container
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables() // Overrides appsettings.json
    .Build();

var serverStatisticsConfig = configuration
    .GetSection(ServerStatisticsConfig.SectionName)
    .Get<ServerStatisticsConfig>();

var rabbitMQOptions = configuration
    .GetSection(RabbitMQOptions.SectionName)
    .Get<RabbitMQOptions>() ?? new RabbitMQOptions();

var statisticsCollectorStrategy = StatisticsCollectorFactory.CreateCollector();

var statisticsCollectorService = new StatisticsCollectorService(statisticsCollectorStrategy);

var serverStatistics = statisticsCollectorService.Collect(serverStatisticsConfig.ServerIdentifier);

Console.WriteLine(serverStatistics);

Console.WriteLine(rabbitMQOptions);

await using IMessagePublisher publisher = await RabbitMQPublisher.CreateAsync(rabbitMQOptions);

var exchangeName = "statistics-exchange";
var queueName = "statistics-collector-queue";
var topicName = $"ServerStatistics.{serverStatisticsConfig.ServerIdentifier}";
var bindingPattern = "ServerStatistics.*";

await publisher.PublishAsync(
    exchange: exchangeName,
    queueName: queueName,
    routingKey: topicName,
    bindingPattern: bindingPattern,
    message: serverStatistics);
