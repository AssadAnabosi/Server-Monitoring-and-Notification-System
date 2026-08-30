using StatisticsCollector.Models;

namespace StatisticsCollector.Utils;

public class StatisticsCollectorService(IStatisticsCollectorStrategy statisticsCollectorStrategyStrategy)
{
    public ServerStatistics Collect(string serverIdentifier)
    {
        return new ServerStatistics
        {
            ServerIdentifier =  serverIdentifier,
            MemoryUsage = statisticsCollectorStrategyStrategy.CalculateMemoryUsage(),
            AvailableMemory = statisticsCollectorStrategyStrategy.CalculateAvailableMemory(),
            CpuUsage = statisticsCollectorStrategyStrategy.CalculateCpuUsages(),
            Timestamp = DateTime.Now
        };
    }
}