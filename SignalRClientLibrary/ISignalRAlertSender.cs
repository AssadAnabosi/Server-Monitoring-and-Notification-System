namespace SignalRClientLibrary;

public interface ISignalRAlertSender
{
    Task SendAsync(AlertEvent alert, CancellationToken cancellationToken = default);
}
