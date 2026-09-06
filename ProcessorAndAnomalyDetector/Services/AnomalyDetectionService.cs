using Microsoft.Extensions.Options;
using ProcessorAndAnomalyDetector.Models;

namespace ProcessorAndAnomalyDetector.Services;

public class AnomalyDetectionService(
    IOptions<AnomalyDetectionConfig> config,
    IServerStatisticsService service)
{
    private readonly AnomalyDetectionConfig _config = config.Value;

    public async Task HandleAsync(ServerStatistics serverStatistics, CancellationToken cancellationToken)
    {
        var previousServerStatistics =
            await service.GetRecentEventAsync(serverStatistics.ServerIdentifier, cancellationToken);
        Console.WriteLine($"Previous server statistics: {previousServerStatistics}");
        Console.WriteLine($"Server statistics: {serverStatistics.ServerIdentifier}");
        await service.InsertAsync(serverStatistics, cancellationToken);

        AnalyseStatistics(serverStatistics, previousServerStatistics);
    }

    private bool IsMemoryHighUsage(double currentMemoryUsage, double currentAvailableMemory)
    {
        var memoryUsageThresholdPercentage = _config.MemoryUsageAnomalyThresholdPercentage;
        return currentMemoryUsage / (currentMemoryUsage + currentAvailableMemory) > memoryUsageThresholdPercentage;
    }

    private bool IsMemoryUsageAnomaly(double currentMemoryUsage, double previousMemoryUsage)
    {
        var memoryUsageThresholdPercentage = _config.MemoryUsageAnomalyThresholdPercentage;
        return currentMemoryUsage > previousMemoryUsage * (1 + memoryUsageThresholdPercentage);
    }

    private bool IsCpuHighUsage(double currentCpuUsage)
    {
        var cpuUsageThresholdPercentage = _config.CpuUsageAnomalyThresholdPercentage;
        return currentCpuUsage > cpuUsageThresholdPercentage * 100;
    }

    private bool IsCpuUsageAnomaly(double currentCpuUsage, double previousCpuUsage)
    {
        var cpuUsageAnomalyThresholdPercentage = _config.CpuUsageAnomalyThresholdPercentage;
        return currentCpuUsage > previousCpuUsage * (1 + cpuUsageAnomalyThresholdPercentage);
    }

    private void AnalyseStatistics(ServerStatistics serverStatistics, ServerStatistics? previousServerStatistics)
    {
        Console.WriteLine("Analysing Statistics...");
        if (IsMemoryHighUsage(serverStatistics.MemoryUsage, serverStatistics.AvailableMemory))
        {
            Console.WriteLine("HighMemoryUsage");
        }

        if (IsCpuHighUsage(serverStatistics.CpuUsage))
        {
            Console.WriteLine("HighCpuUsage");
        }

        Console.WriteLine("Detecting Anomalies...");
        if (previousServerStatistics is null)
            return;

        if (IsMemoryUsageAnomaly(serverStatistics.MemoryUsage, previousServerStatistics.MemoryUsage))
        {
            Console.WriteLine("MemoryUsageAnomaly");
        }

        if (IsCpuUsageAnomaly(serverStatistics.CpuUsage, previousServerStatistics.CpuUsage))
        {
            Console.WriteLine("CpuUsageAnomaly");
        }
    }
}