using ProcessorAndAnomalyDetector.Models;

namespace ProcessorAndAnomalyDetector.Repositories;

public interface IServerStatisticsRepository
{
    Task InsertAsync(ServerStatisticsEntity entity, CancellationToken cancellationToken);
}