using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StatisticsCollector.Models;
using StatisticsCollector.Utils;
using RabbitMQClientLibrary;
using StatisticsCollector;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<ServerStatisticsConfig>()
    .BindConfiguration(ServerStatisticsConfig.SectionName)
    .Validate(c => c is not null, $"Missing configuration section '{ServerStatisticsConfig.SectionName}'.");

builder.Services
    .AddOptions<RabbitMQOptions>()
    .BindConfiguration(RabbitMQOptions.SectionName);

builder.Services.AddSingleton(_ => StatisticsCollectorFactory.CreateCollector());
builder.Services.AddSingleton<StatisticsCollectorService>();

builder.Services.AddHostedService<StatisticsPublisherHostedService>();

var host = builder.Build();
await host.RunAsync();