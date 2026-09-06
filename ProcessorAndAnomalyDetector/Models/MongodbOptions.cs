namespace ProcessorAndAnomalyDetector.Models;

public class MongoDbOptions
{
    public const string SectionName = "MongoDbOptions";
    public string ConnectionString { get; set; } = "mongodb://root:root@localhost:27017/?authSource=admin";
    public string Database { get; set; } = "base";
    public string Collection { get; set; } = "serverStatistics";
}