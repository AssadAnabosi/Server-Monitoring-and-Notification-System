using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace SignalRClientLibrary;

public sealed class SignalRAlertReceiver : ISignalRAlertReceiver, IAsyncDisposable
{
    private readonly HubConnection _connection;

    public SignalRAlertReceiver(IOptions<SignalRHubOptions> options)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(options.Value.Url)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<AlertEvent>("AlertReceived", alert =>
            AlertReceived?.Invoke(alert) ?? Task.CompletedTask);
    }

    public event Func<AlertEvent, Task>? AlertReceived;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
