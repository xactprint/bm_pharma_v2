# BM-PHASE-011-04-DESIGN-MONITORING — Monitoring Infrastructure

## Components

### 1. ChifaCircuitBreaker
```
States: Closed → Open (after FailureThreshold failures)
             → HalfOpen (after OpenTimeoutMs)
             → Closed (after SuccessThreshold successes)
```
- Defaults: FailureThreshold=3, SuccessThreshold=2, OpenTimeoutMs=30s
- Per-key (e.g. "chifa", "token")
- Thread-safe via `ConcurrentDictionary`

### 2. ChifaMetricsService
- Collects `ChifaMetricPoint` (operation, success, durationMs, timestamp)
- `GetSummary(operation?)` → (total, success, failed, avgMs)
- Resetable

### 3. ChifaHealthCheckService
- Wraps `IChifaIntegrationService.GetHealthStatusAsync()`
- Returns `ChifaHealthStatus` (IsOnline, IsDatabaseConnected, IsTokenPresent, ErrorMessage, CheckedAt)

### 4. ChifaMonitoringService
- Orchestrates all monitoring components
- Optional timer-based health check (default 60s interval)
- `LastOperation` / `LastSyncResult` tracking
- `GetMetrics()` / `GetCircuitState()` / `ResetCircuitBreaker()` convenience methods

### 5. CorrelationContext
- `AsyncLocal<string>` per-operation correlation ID
- `GetOrCreate()` returns existing or new (from `Activity.Current?.Id` or GUID)
- `Reset()` clears for next operation

## Data Flow

```
Request → ChifaIntegrationFacade
  ├── CorrelationContext.GetOrCreate() → correlationId
  ├── (business logic)
  ├── ChifaMetricsService.Record(op, success, duration)
  ├── ChifaCircuitBreaker.RecordSuccess/Failure(key)
  └── IChifaAuditService.LogOperationAsync(..., correlationId)
```

## Timing

- All facade operations include `DurationMs` in result
- `ChifaSyncSummary.DurationMs` tracks full sync time
- `ChifaStatusSnapshot.DurationMs` tracks status evaluation time
