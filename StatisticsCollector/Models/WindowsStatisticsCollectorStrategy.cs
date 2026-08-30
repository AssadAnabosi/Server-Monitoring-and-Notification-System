using System.Management;
using System.Diagnostics;

namespace StatisticsCollector.Models;

public class WindowsStatisticsCollectorStrategy : IStatisticsCollectorStrategy
{
    public float CalculateMemoryUsage()
    {
        using var computer = new ManagementObjectSearcher(
            "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

        using var result = computer.Get().Cast<ManagementObject>().First();

        var totalMemory = Convert.ToDouble(result["TotalVisibleMemorySize"]);
        var freeMemory = Convert.ToDouble(result["FreePhysicalMemory"]);

        return (float)(totalMemory - freeMemory);
    }

    public float CalculateAvailableMemory()
    {
        using var computer = new ManagementObjectSearcher(
            "SELECT FreePhysicalMemory FROM Win32_OperatingSystem");

        using var result = computer.Get().Cast<ManagementObject>().First();

        return Convert.ToSingle(result["FreePhysicalMemory"]);
    }

    public float CalculateCpuUsages(int numberOfTimes = 5, int sleepTime = 200)
    {
        using var cpuCounter = new PerformanceCounter(
            "Processor",
            "% Processor Time",
            "_Total");

        // First call initializes the counter.
        cpuCounter.NextValue();

        Thread.Sleep(sleepTime);

        var total = 0f;

        for (var i = 0; i < numberOfTimes; i++)
        {
            total += cpuCounter.NextValue();

            if (i < numberOfTimes - 1)
                Thread.Sleep(sleepTime);
        }

        return total / numberOfTimes;
    }
}