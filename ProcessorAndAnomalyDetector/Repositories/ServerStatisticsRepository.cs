using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProcessorAndAnomalyDetector.Models;

namespace ProcessorAndAnomalyDetector.Repositories;

public sealed class ServerStatisticsRepository : IServerStatisticsRepository
{
    private readonly IMongoCollection<ServerStatistics>? _collection;

    public ServerStatisticsRepository(IOptions<MongoDbOptions> mongoOptions)
    {
        var options = mongoOptions.Value;
        _collection = ConnectToDatabase(options);
    }

    private IMongoCollection<ServerStatistics>? ConnectToDatabase(MongoDbOptions options)
    {
        try
        {
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.Database);
            return database.GetCollection<ServerStatistics>(options.Collection);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to connect to the database");
            Console.WriteLine(e.Message);
        }

        return null;
    }

    public async Task InsertAsync(ServerStatistics entity, CancellationToken cancellationToken)
    {
        try
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to insert server statistics");
            Console.WriteLine(e.Message);
        }
    }

    public async Task<ServerStatistics?> GetRecentEventAsync(string serverIdentifier,
        CancellationToken cancellationToken)
    {
        try
        {
            const string searchField = "ServerIdentifier";
            var filter = Builders<ServerStatistics>.Filter.Eq(searchField, serverIdentifier);
            return await _collection.Find(filter).Sort(Builders<ServerStatistics>.Sort.Descending(e => e.Timestamp))
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to fetch server statistics");
            Console.WriteLine(e.Message);
        }

        return null;
    }
}