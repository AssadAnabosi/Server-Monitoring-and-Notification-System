namespace StatisticsCollector.Models;

public class ServerStatisticsConfig
{
    public const string SectionName = "ServerStatisticsConfig";
    
    public int SamplingIntervalSeconds { get; set; }
    public string ServerIdentifier { get; set; } = string.Empty;

    override public string ToString() =>
     $"Server Identifier: {this.ServerIdentifier}\n" +
     $"Sampling Interval: {this.SamplingIntervalSeconds}";
}