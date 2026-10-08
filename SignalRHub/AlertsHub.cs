using Microsoft.AspNetCore.SignalR;
using SignalRClientLibrary;

namespace SignalRHub;

public sealed class AlertsHub : Hub
{
    public Task PublishAlert(AlertEvent alert)
    {
        return Clients.All.SendAsync("AlertReceived", alert);
    }
}
