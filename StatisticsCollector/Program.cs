using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;
using RabbitMQClientLibrary;
using StatisticsCollector;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddOptions<ServerStatisticsConfig>()
    .Bind(builder.Configuration.GetSection(ServerStatisticsConfig.SectionName))
    .Validate(c => c is not null, $"Missing configuration section '{ServerStatisticsConfig.SectionName}'.");

builder.Services
    .AddOptions<RabbitMQOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMQOptions.SectionName));

builder.Services.AddSingleton(_ => StatisticsCollectorFactory.CreateCollector());
builder.Services.AddSingleton<StatisticsCollectorService>();

builder.Services.AddHostedService<StatisticsPublisherHostedService>();

var host = builder.Build();
await host.RunAsync();