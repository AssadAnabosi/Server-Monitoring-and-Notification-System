using MongoDB.Bson;
using ProcessorAndAnomalyDetector.Models;
using ProcessorAndAnomalyDetector.Repositories;

namespace ProcessorAndAnomalyDetector;

public sealed class ServerStatisticsMessageHandler
{
    private readonly IServerStatisticsRepository _repository;

    public ServerStatisticsMessageHandler(
        IServerStatisticsRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        StatisticsCollector.Models.ServerStatistics message,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(
            $"Received ServerStatistics message from {message.ServerIdentifier}");

        var entity = new ServerStatisticsEntity
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ServerIdentifier = message.ServerIdentifier,
            MemoryUsage = message.MemoryUsage,
            AvailableMemory = message.AvailableMemory,
            CpuUsage = message.CpuUsage,
            Timestamp = message.Timestamp
        };

        try
        {
            await _repository.InsertAsync(entity, cancellationToken);

            Console.WriteLine(
                $"Persisted ServerStatistics with id '{entity.Id}' for {entity.ServerIdentifier}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Failed to persist ServerStatistics message from {message.ServerIdentifier}");

            Console.WriteLine($"Error: {ex}");

            // Rethrow so the consumer library's ack/nack policy can decide whether to requeue.
            throw;
        }
    }
}