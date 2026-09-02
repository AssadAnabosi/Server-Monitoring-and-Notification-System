namespace ProcessorAndAnomalyDetector.Models;

public class AnomalyDetectionConfig
{
    public const string SectionName = "AnomalyDetectionConfig";
    public double MemoryUsageAnomalyThresholdPercentage { get; set; }
    public double CpuUsageAnomalyThresholdPercentage  { get; set; }
    public double MemoryUsageThresholdPercentage  { get; set; }
    public double CpuUsageThresholdPercentage   { get; set; }

    public override string ToString() =>
        $"Memory Usage Threshold Percentage: {MemoryUsageThresholdPercentage}{Environment.NewLine}" +
        $"CPU Usage Threshold Percentage: {CpuUsageThresholdPercentage}{Environment.NewLine}" +
        $"Memory Usage Anomaly Threshold Percentage: {MemoryUsageAnomalyThresholdPercentage}{Environment.NewLine}" + 
        $"CPU Usage Anomaly Threshold Percentage: {CpuUsageAnomalyThresholdPercentage}{Environment.NewLine}";
}