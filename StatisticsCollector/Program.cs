using Microsoft.Extensions.Configuration;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;

// TODO: DI Container
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables() // Overrides appsettings.json
    .Build();

var config = configuration
    .GetSection(ServerStatisticsConfig.SectionName)
    .Get<ServerStatisticsConfig>();

var statisticsCollectorStrategy = StatisticsCollectorFactory.CreateCollector();

var statisticsCollectorService = new StatisticsCollectorService(statisticsCollectorStrategy);

var serverStatistics = statisticsCollectorService.Collect(config.ServerIdentifier);

Console.WriteLine(serverStatistics);