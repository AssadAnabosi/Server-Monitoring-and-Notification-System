using Microsoft.Extensions.Options;
using ProcessorAndAnomalyDetector.Models;
using SignalRClientLibrary;

namespace ProcessorAndAnomalyDetector.Services;

public class AnomalyDetectionService(
    IOptions<AnomalyDetectionConfig> config,
    IServerStatisticsService service,
    ISignalRAlertSender alertSender)
{
    private readonly AnomalyDetectionConfig _config = config.Value;

    public async Task HandleAsync(ServerStatistics serverStatistics, CancellationToken cancellationToken)
    {
        var previousServerStatistics =
            await service.GetRecentEventAsync(serverStatistics.ServerIdentifier, cancellationToken);
        Console.WriteLine($"Previous server statistics: {previousServerStatistics}");
        Console.WriteLine($"Server statistics: {serverStatistics.ServerIdentifier}");
        await service.InsertAsync(serverStatistics, cancellationToken);

        await AnalyseStatisticsAsync(serverStatistics, previousServerStatistics, cancellationToken);
    }

    private bool IsMemoryUsageAnomaly(double currentMemoryUsage, double previousMemoryUsage)
    {
        var memoryUsageThresholdPercentage = _config.MemoryUsageAnomalyThresholdPercentage;
        return currentMemoryUsage > previousMemoryUsage * (1 + memoryUsageThresholdPercentage);
    }

    private bool IsCpuHighUsage(double currentCpuUsage)
    {
        var cpuUsageThresholdPercentage = _config.CpuUsageThresholdPercentage;
        return currentCpuUsage > cpuUsageThresholdPercentage * 100;
    }

    private bool IsCpuUsageAnomaly(double currentCpuUsage, double previousCpuUsage)
    {
        var cpuUsageAnomalyThresholdPercentage = _config.CpuUsageAnomalyThresholdPercentage;
        return currentCpuUsage > previousCpuUsage * (1 + cpuUsageAnomalyThresholdPercentage);
    }

    private async Task AnalyseStatisticsAsync(
        ServerStatistics serverStatistics,
        ServerStatistics? previousServerStatistics,
        CancellationToken cancellationToken)
    {
        var totalMemory = serverStatistics.MemoryUsage + serverStatistics.AvailableMemory;
        var memoryUsagePercentage = totalMemory > 0 ? serverStatistics.MemoryUsage / totalMemory : 0;
        var cpuUsagePercentage = serverStatistics.CpuUsage / 100;

        if (memoryUsagePercentage > _config.MemoryUsageThresholdPercentage ||
            IsCpuHighUsage(serverStatistics.CpuUsage))
        {
            await SendAlertAsync(
                "High Usage Alert",
                serverStatistics,
                memoryUsagePercentage,
                cpuUsagePercentage,
                $"Usage threshold exceeded. Memory: {memoryUsagePercentage:P1}, CPU: {cpuUsagePercentage:P1}.",
                cancellationToken);
            Console.WriteLine("High Usage Alert");
        }

        if (previousServerStatistics is null)
            return;

        var memoryAnomaly = IsMemoryUsageAnomaly(serverStatistics.MemoryUsage, previousServerStatistics.MemoryUsage);
        var cpuAnomaly = IsCpuUsageAnomaly(serverStatistics.CpuUsage, previousServerStatistics.CpuUsage);
        if (memoryAnomaly || cpuAnomaly)
        {
            await SendAlertAsync(
                "Anomaly Alert",
                serverStatistics,
                memoryUsagePercentage,
                cpuUsagePercentage,
                $"Sudden increase detected in {(memoryAnomaly && cpuAnomaly ? "memory and CPU" : memoryAnomaly ? "memory" : "CPU")} usage.",
                cancellationToken);
            Console.WriteLine("Anomaly Alert");
        }
    }

    private Task SendAlertAsync(
        string type,
        ServerStatistics statistics,
        double memoryUsagePercentage,
        double cpuUsagePercentage,
        string message,
        CancellationToken cancellationToken)
    {
        return alertSender.SendAsync(new AlertEvent
        {
            Type = type,
            ServerIdentifier = statistics.ServerIdentifier,
            MemoryUsagePercentage = memoryUsagePercentage,
            CpuUsagePercentage = cpuUsagePercentage,
            Message = message,
            Timestamp = statistics.Timestamp == default ? DateTime.UtcNow : statistics.Timestamp
        }, cancellationToken);
    }
}