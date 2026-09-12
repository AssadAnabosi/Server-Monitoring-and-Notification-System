using ProcessorAndAnomalyDetector.Models;

namespace ProcessorAndAnomalyDetector.Repositories;

public interface IServerStatisticsRepository
{
    Task InsertAsync(ServerStatistics entity, CancellationToken cancellationToken);

    Task<ServerStatistics?> GetRecentEventAsync(string serverIdentifier, CancellationToken cancellationToken);
}