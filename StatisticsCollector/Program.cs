using Microsoft.Extensions.Configuration;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var config = configuration
    .GetSection("ServerStatisticsConfig")
    .Get<ServerStatisticsConfig>();

var statisticsCollectorStrategy = StatisticsCollectorFactory.CreateCollector();

var statisticsCollectorService = new StatisticsCollectorService(statisticsCollectorStrategy);

var serverStatistics = statisticsCollectorService.Collect(config.ServerIdentifier);

Console.WriteLine(serverStatistics);