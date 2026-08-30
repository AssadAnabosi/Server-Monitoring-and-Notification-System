namespace StatisticsCollector.Models;

public class UnixStatisticsCollectorStrategy : IStatisticsCollectorStrategy
{
    public float CalculateMemoryUsage()
    {
        var memory = ReadMemoryInfo();

        return memory.Total - memory.Available;
    }

    public float CalculateAvailableMemory()
    {
        var memory = ReadMemoryInfo();

        return memory.Available;
    }

    public float CalculateCpuUsages(int numberOfTimes = 5, int sleepTime = 200)
    {
        var usages = new List<float>();

        var previous = ReadCpuStats();

        for (var i = 0; i < numberOfTimes; i++)
        {
            Thread.Sleep(sleepTime);

            var current = ReadCpuStats();

            var idleDelta = current.Idle - previous.Idle;
            var totalDelta = current.Total - previous.Total;

            var usage = totalDelta == 0
                ? 0
                : 100f * (1f - (float)idleDelta / totalDelta);

            usages.Add(usage);

            previous = current;
        }

        return usages.Average();
    }

    private static (float Total, float Available) ReadMemoryInfo()
    {
        var lines = File.ReadAllLines("/proc/meminfo");

        float total = 0;
        float available = 0;

        foreach (var line in lines)
        {
            var parts = line.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                continue;

            var value = float.Parse(parts[1]);

            switch (parts[0])
            {
                case "MemTotal:":
                    total = value;
                    break;

                case "MemAvailable:":
                    available = value;
                    break;
            }
        }

        return (total, available);
    }

    private static (long Idle, long Total) ReadCpuStats()
    {
        var line = File.ReadLines("/proc/stat")
            .First(x => x.StartsWith("cpu "));

        var parts = line.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        var values = parts
            .Skip(1)
            .Select(long.Parse)
            .ToArray();

        var idle = values[3] + values[4]; // idle + iowait
        var total = values.Sum();

        return (idle, total);
    }
}