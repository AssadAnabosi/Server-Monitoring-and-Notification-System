namespace SignalRClientLibrary;

public interface ISignalRAlertReceiver
{
    event Func<AlertEvent, Task>? AlertReceived;

    Task StartAsync(CancellationToken cancellationToken = default);
}
