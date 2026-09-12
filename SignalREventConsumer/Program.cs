using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRClientLibrary;
using SignalREventConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<SignalRHubOptions>()
    .BindConfiguration(SignalRHubOptions.SectionName);

builder.Services.AddSingleton<SignalRAlertReceiver>();
builder.Services.AddHostedService<SignalRAlertWorker>();

await builder.Build().RunAsync();