using Microsoft.Extensions.Hosting;
using SignalRClientLibrary;

namespace SignalREventConsumer;

public sealed class SignalRAlertWorker(
    SignalRAlertReceiver receiver) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        receiver.AlertReceived += OnAlertReceived;

        try
        {
            await receiver.StartAsync(cancellationToken: cancellationToken);

            Console.WriteLine(
                "Connected to the SignalR alert hub. Waiting for events...");

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        finally
        {
            receiver.AlertReceived -= OnAlertReceived;
        }
    }

    private static Task OnAlertReceived(AlertEvent alert)
    {
        Console.WriteLine(
            $"[{alert.Timestamp:O}] {alert.Type} - {alert.ServerIdentifier}");

        Console.WriteLine(alert.Message);
        Console.WriteLine(
            $"Memory: {alert.MemoryUsagePercentage:P1}, " +
            $"CPU: {alert.CpuUsagePercentage:P1}");

        return Task.CompletedTask;
    }
}
