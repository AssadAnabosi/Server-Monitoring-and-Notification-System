namespace StatisticsCollector.Models;

public class ServerStatistics
{
    public String ServerIdentifier { get; set; } = String.Empty;
    public double MemoryUsage { get; set; } // in MB
    public double AvailableMemory { get; set; } // in MB
    public double CpuUsage { get; set; }
    public DateTime Timestamp { get; set; }

    public override string ToString() =>
        $"{{\n\t\"ServerIdentifier\": \"{ServerIdentifier}\",\n\t\"MemoryUsage\": {MemoryUsage},\n\t\"AvailableMemory\": {AvailableMemory},\n\t\"CpuUsage\": {CpuUsage},\n\t\"Timestamp\": \"{Timestamp:yyyy-MM-ddTHH:mm:ssZ}\"\n}}";
}