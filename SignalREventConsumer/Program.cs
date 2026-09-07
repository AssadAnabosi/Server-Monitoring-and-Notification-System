using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalRClientLibrary;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);

services.AddOptions<SignalRHubOptions>()
    .BindConfiguration(SignalRHubOptions.SectionName);

services.AddSingleton<SignalRAlertReceiver>();

await using var provider = services.BuildServiceProvider();

var receiver = provider.GetRequiredService<SignalRAlertReceiver>();
receiver.AlertReceived += alert =>
{
    Console.WriteLine($"[{alert.Timestamp:O}] {alert.Type} - {alert.ServerIdentifier}");
    Console.WriteLine(alert.Message);
    Console.WriteLine($"Memory: {alert.MemoryUsagePercentage:P1}, CPU: {alert.CpuUsagePercentage:P1}");
    return Task.CompletedTask;
};

await receiver.StartAsync();
Console.WriteLine("Connected to the SignalR alert hub. Waiting for events...");
Console.WriteLine("Press Ctrl+C to exit.");

var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    completion.TrySetResult();
};

await completion.Task;