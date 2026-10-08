namespace SignalRClientLibrary;

public sealed class AlertEvent
{
    public string Type { get; init; } = string.Empty;
    public string ServerIdentifier { get; init; } = string.Empty;
    public double MemoryUsagePercentage { get; init; }
    public double CpuUsagePercentage { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
