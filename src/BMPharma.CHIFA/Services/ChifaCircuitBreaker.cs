using System.Collections.Concurrent;

namespace BMPharma.CHIFA.Services;

public enum CircuitState { Closed, Open, HalfOpen }

public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 3;
    public int SuccessThreshold { get; set; } = 2;
    public int OpenTimeoutMs { get; set; } = 30_000;
    public int HalfOpenTimeoutMs { get; set; } = 5_000;
}

public class ChifaCircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private readonly ConcurrentDictionary<string, CircuitState> _states = new();
    private readonly ConcurrentDictionary<string, int> _failureCounts = new();
    private readonly ConcurrentDictionary<string, int> _successCounts = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastFailureTime = new();

    public ChifaCircuitBreaker(CircuitBreakerOptions? options = null)
    {
        _options = options ?? new CircuitBreakerOptions();
    }

    public CircuitState GetState(string key) =>
        _states.TryGetValue(key, out var state) ? EvaluateState(key, state) : CircuitState.Closed;

    public bool IsOpen(string key) => GetState(key) == CircuitState.Open;

    public void RecordSuccess(string key)
    {
        _successCounts.AddOrUpdate(key, 1, (_, c) => c + 1);
        _failureCounts.TryRemove(key, out _);
        if (_successCounts[key] >= _options.SuccessThreshold)
        {
            _states[key] = CircuitState.Closed;
            _successCounts.TryRemove(key, out _);
        }
    }

    public void RecordFailure(string key)
    {
        _failureCounts.AddOrUpdate(key, 1, (_, c) => c + 1);
        _lastFailureTime[key] = DateTime.UtcNow;
        if (_failureCounts[key] >= _options.FailureThreshold)
            _states[key] = CircuitState.Open;
    }

    public void Reset(string key)
    {
        _states.TryRemove(key, out _);
        _failureCounts.TryRemove(key, out _);
        _successCounts.TryRemove(key, out _);
        _lastFailureTime.TryRemove(key, out _);
    }

    private CircuitState EvaluateState(string key, CircuitState current)
    {
        if (current != CircuitState.Open) return current;
        if (_lastFailureTime.TryGetValue(key, out var lastFail))
        {
            if ((DateTime.UtcNow - lastFail).TotalMilliseconds >= _options.OpenTimeoutMs)
            {
                _states[key] = CircuitState.HalfOpen;
                return CircuitState.HalfOpen;
            }
        }
        return CircuitState.Open;
    }
}
