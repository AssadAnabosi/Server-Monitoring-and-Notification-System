using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcessorAndAnomalyDetector.Models;
using ProcessorAndAnomalyDetector.Repositories;
using ProcessorAndAnomalyDetector.Services;
using RabbitMQClientLibrary;
using SignalRClientLibrary;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<RabbitMQOptions>()
    .BindConfiguration(RabbitMQOptions.SectionName);

builder.Services
    .AddOptions<MongoDbOptions>()
    .BindConfiguration(MongoDbOptions.SectionName);

builder.Services
    .AddOptions<AnomalyDetectionConfig>()
    .BindConfiguration(AnomalyDetectionConfig.SectionName)
    .Validate(c => c is not null, $"Missing configuration section '{AnomalyDetectionConfig.SectionName}'.");

builder.Services
    .AddOptions<SignalRHubOptions>()
    .BindConfiguration(SignalRHubOptions.SectionName);

builder.Services.AddSingleton<IServerStatisticsRepository, ServerStatisticsRepository>();
builder.Services.AddSingleton<IServerStatisticsService, ServerStatisticsService>();
builder.Services.AddSingleton<ISignalRAlertSender, SignalRAlertSender>();
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddHostedService<StatisticsConsumerHostedService>();

var host = builder.Build();
await host.RunAsync();