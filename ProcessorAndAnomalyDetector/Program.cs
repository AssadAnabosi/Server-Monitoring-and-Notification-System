using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcessorAndAnomalyDetector.Models;
using ProcessorAndAnomalyDetector.Repositories;
using ProcessorAndAnomalyDetector.Services;
using RabbitMQClientLibrary;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddOptions<RabbitMQOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMQOptions.SectionName));

builder.Services
    .AddOptions<MongoDbOptions>()
    .Bind(builder.Configuration.GetSection(MongoDbOptions.SectionName));

builder.Services
    .AddOptions<AnomalyDetectionConfig>()
    .Bind(builder.Configuration.GetSection(AnomalyDetectionConfig.SectionName))
    .Validate(c => c is not null, $"Missing configuration section '{AnomalyDetectionConfig.SectionName}'.");


builder.Services.AddSingleton<IServerStatisticsRepository, ServerStatisticsRepository>();
builder.Services.AddSingleton<IServerStatisticsService, ServerStatisticsService>();
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddHostedService<StatisticsConsumerHostedService>();

var host = builder.Build();
await host.RunAsync();