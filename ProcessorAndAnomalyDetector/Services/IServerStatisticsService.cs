using ProcessorAndAnomalyDetector.Models;

namespace ProcessorAndAnomalyDetector.Services;

public interface IServerStatisticsService
{
    Task InsertAsync(
        ServerStatistics entity,
        CancellationToken cancellationToken);

    Task<ServerStatistics?> GetRecentEventAsync(
        string serverIdentifier,
        CancellationToken cancellationToken);
}