using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace SignalRClientLibrary;

public sealed class SignalRAlertSender : ISignalRAlertSender, IAsyncDisposable
{
    private readonly HubConnection _connection;

    public SignalRAlertSender(IOptions<SignalRHubOptions> options)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(options.Value.Url)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task SendAsync(AlertEvent alert, CancellationToken cancellationToken = default)
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync(cancellationToken);

        await _connection.InvokeAsync("PublishAlert", alert, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
