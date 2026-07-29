using System.Diagnostics;

namespace BMPharma.CHIFA.Services;

public class ChifaMonitoringService : IDisposable
{
    private readonly ChifaHealthCheckService _healthCheck;
    private readonly ChifaMetricsService _metrics;
    private readonly ChifaCircuitBreaker _circuitBreaker;
    private readonly CorrelationContext _correlation;
    private ChifaSyncSummary? _lastSyncResult;
    private string? _lastOperation;
    private readonly Timer? _healthTimer;

    public string? LastOperation => _lastOperation;
    public ChifaSyncSummary? LastSyncResult => _lastSyncResult;

    public ChifaMonitoringService(
        ChifaHealthCheckService healthCheck,
        ChifaMetricsService metrics,
        ChifaCircuitBreaker circuitBreaker,
        CorrelationContext correlation)
    {
        _healthCheck = healthCheck;
        _metrics = metrics;
        _circuitBreaker = circuitBreaker;
        _correlation = correlation;
    }

    public ChifaMonitoringService(
        ChifaHealthCheckService healthCheck,
        ChifaMetricsService metrics,
        ChifaCircuitBreaker circuitBreaker,
        CorrelationContext correlation,
        int healthCheckIntervalMs = 60_000)
        : this(healthCheck, metrics, circuitBreaker, correlation)
    {
        if (healthCheckIntervalMs > 0)
        {
            _healthTimer = new Timer(
                async _ => await RunHealthCheckAsync().ConfigureAwait(false),
                null, healthCheckIntervalMs, healthCheckIntervalMs);
        }
    }

    public void RecordOperation(string operation)
    {
        _lastOperation = operation;
    }

    public void RecordSyncResult(ChifaSyncSummary summary)
    {
        _lastSyncResult = summary;
        _lastOperation = "Synchronize";
    }

    public async Task<ChifaSyncSummary?> GetLastSyncResultAsync(
        CancellationToken _ = default)
    {
        return _lastSyncResult;
    }

    public (int total, int success, int failed, double avgMs) GetMetrics(string? operation = null)
    {
        return _metrics.GetSummary(operation);
    }

    public void ResetCircuitBreaker(string key)
    {
        _circuitBreaker.Reset(key);
    }

    public CircuitState GetCircuitState(string key)
    {
        return _circuitBreaker.GetState(key);
    }

    private async Task RunHealthCheckAsync()
    {
        try
        {
            _correlation.GetOrCreate();
            var sw = Stopwatch.StartNew();
            await _healthCheck.CheckAllAsync().ConfigureAwait(false);
            _metrics.Record("HealthCheck", true, sw.ElapsedMilliseconds);
        }
        catch
        {
            _metrics.Record("HealthCheck", false, 0);
        }
    }

    public void Dispose()
    {
        _healthTimer?.Dispose();
    }
}
