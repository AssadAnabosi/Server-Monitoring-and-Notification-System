using MongoDB.Driver;
using ProcessorAndAnomalyDetector.Models;

namespace ProcessorAndAnomalyDetector.Repositories;

public sealed class ServerStatisticsRepository : IServerStatisticsRepository
{
    private readonly IMongoCollection<ServerStatisticsEntity> _collection;

    public ServerStatisticsRepository(MongoDbOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var client = new MongoClient(options.ConnectionString);
        var database = client.GetDatabase(options.Database);
        _collection = database.GetCollection<ServerStatisticsEntity>(options.Collection);
    }

    public Task InsertAsync(ServerStatisticsEntity entity, CancellationToken cancellationToken)
        => _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
}