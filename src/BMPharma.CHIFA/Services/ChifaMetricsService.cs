using System.Collections.Concurrent;

namespace BMPharma.CHIFA.Services;

public class ChifaMetricPoint
{
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ChifaMetricsService
{
    private readonly ConcurrentBag<ChifaMetricPoint> _points = new();

    public void Record(string operation, bool success, long durationMs)
    {
        _points.Add(new ChifaMetricPoint
        {
            Operation = operation, Success = success,
            DurationMs = durationMs, Timestamp = DateTime.UtcNow
        });
    }

    public IReadOnlyList<ChifaMetricPoint> GetAll() => _points.ToList().AsReadOnly();

    public (int total, int success, int failed, double avgMs) GetSummary(string? operation = null)
    {
        var filtered = operation == null ? _points : _points.Where(p => p.Operation == operation);
        var list = filtered.ToList();
        if (list.Count == 0) return (0, 0, 0, 0);
        return (list.Count, list.Count(p => p.Success), list.Count(p => !p.Success),
                list.Average(p => p.DurationMs));
    }

    public void Reset()
    {
        while (_points.TryTake(out _)) { }
    }
}
