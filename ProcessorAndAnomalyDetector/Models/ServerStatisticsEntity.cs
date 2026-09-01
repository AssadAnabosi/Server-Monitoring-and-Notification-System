using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProcessorAndAnomalyDetector.Models;

public class ServerStatisticsEntity : StatisticsCollector.Models.ServerStatistics
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
}