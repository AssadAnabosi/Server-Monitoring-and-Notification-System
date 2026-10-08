using ProcessorAndAnomalyDetector.Models;
using ProcessorAndAnomalyDetector.Repositories;

namespace ProcessorAndAnomalyDetector.Services;

public sealed class ServerStatisticsService(IServerStatisticsRepository repository) : IServerStatisticsService
{
    public async Task InsertAsync(
        ServerStatistics entity,
        CancellationToken cancellationToken)
    {
        try
        {
            await repository.InsertAsync(entity, cancellationToken);

            Console.WriteLine(
                $"Persisted ServerStatistics with id '{entity.Id}' for {entity.ServerIdentifier}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Failed to persist ServerStatistics message from {entity.ServerIdentifier}");

            Console.WriteLine($"Error: {ex}");

            // Rethrow so the consumer library's ack/nack policy can decide whether to requeue.
            throw;
        }
    }

    public async Task<ServerStatistics?> GetRecentEventAsync(
        string serverIdentifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serverIdentifier);

        return await repository.GetRecentEventAsync(
            serverIdentifier,
            cancellationToken);
    }
}