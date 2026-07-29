# Phase 013 — Application Integration (UI → EF Core → CHIFA)

**Status:** ✅ Complete  
**Date:** 2026-07-29  
**Goal:** Connect ViewModels to real facade data, display monitoring metrics, error mapping, UI tests.

---

## Changes Summary

### 1. `DashboardOverview` DTO Enrichment (`IChifaIntegrationFacade.cs`)
- Added `CircuitBreakerState`, `CircuitBreakerKey`, `CircuitBreakerFailureCount`
- Added `MetricsTotal`, `MetricsSuccess`, `MetricsFailed`, `MetricsAvgMs`
- Added `CorrelationId`, `LastSyncResult`, `LastSyncTime`

### 2. `StatusSynchronizer` — Real Invoice Count (`StatusSynchronizer.cs`)
- Added `CountInvoicesTodayAsync()` — queries DB for today's invoice count (real data)

### 3. `ChifaIntegrationFacade.GetDashboardOverviewAsync()` — Real Data Population
- `TotalInvoicesToday` → from `StatusSynchronizer.CountInvoicesTodayAsync()`
- `PendingBordereaux` → from `GetAllBordereauxAsync()` filtered by non-transmitted/non-closed
- `CorrelationId` → from `CorrelationContext.Current`
- `CircuitBreakerState` → from `MonitoringService.GetCircuitState()`
- `Metrics*` → from `MonitoringService.GetMetrics()`
- `LastSyncResult`, `LastSyncTime` → from `LastSyncResult`

### 4. `ChifaExceptionMapper` (New — `ChifaExceptionMapper.cs`)
Maps exception types to user-friendly French messages:
| Exception | User Message | Error Code |
|-----------|-------------|------------|
| `HttpRequestException` | "Erreur réseau CHIFA (HTTP {code}): {message}" | `HTTP_ERROR` |
| `TaskCanceledException` | "La demande a expiré..." | `TIMEOUT` |
| `TimeoutException` | "La demande a expiré..." | `TIMEOUT` |
| `OperationCanceledException` | "L'opération a été annulée." | `CANCELED` |
| `InvalidOperationException` | "Erreur de token..." / "Opération invalide..." | `INVALID_OPERATION` |
| `UnauthorizedAccessException` | "Accès refusé..." | `ACCESS_DENIED` |
| Generic | Falls back to inner exception or ex.Message | `SYSTEM_ERROR` |

### 5. `ChifaDashboardViewModel` — Full Rewrite
- **Real data**: Uses `GetDashboardOverviewAsync()` as single source of truth
- **Monitoring display**: Circuit breaker state (color-coded: green=Closed, orange=HalfOpen, red=Open)
- **Metrics**: Total/success/failed/avg time display
- **Correlation ID**: Shown for traceability
- **Last sync result**: Summary of last synchronization
- **Auto-refresh**: Timer every 30 seconds updates the dashboard
- **Workflow steps**: Derived from real data (invoice counts, sync counts, bordereau counts)

### 6. `ChifaInvoicePreparationViewModel` — Enhanced
- **Preview step**: `PreviewInvoice()` / `HidePreview()` commands toggle preview overlay showing line count, validation rules, mode notice, and computed amounts
- **ReadOnly mode guard**: Both `ValidateInvoiceAsync` and `PrepareAndSubmitAsync` check ReadOnly mode and show simulated error instead of attempting writes
- **Exception handling**: Uses `ex.ToUserMessage()` instead of raw "Erreur inattendue: {ex.Message}"
- **Error codes**: Uses `ex.GetErrorCode()` for structured error reporting
- **`AddSimulatedError()`**: Helper to add structured error entries

### 7. UI Tests (`ChifaPhase013IntegrationTests.cs`) — 24 Test Cases
| Test ID | Description |
|---------|-------------|
| PH013_DASH001 | GetDashboardOverview returns Success when available |
| PH013_DASH002 | DashboardOverview contains CorrelationId + CircuitBreakerState |
| PH013_DASH003 | DashboardOverview contains Metrics |
| PH013_DASH004 | DashboardOverview returns Degraded when CHIFA unavailable |
| PH013_DASH005 | Default circuit breaker state is "Closed" |
| PH013_DASH006 | LastOperation is recorded after each call |
| PH013_DASH007 | LastSyncResult string is present |
| PH013_EXC001–010 | Exception mapper tests (all exception types) |
| PH013_MODE001–002 | ReadOnly/Production mode integration tests |
| PH013_WORKFLOW001–003 | Workflow state machine, ReadOnly guard |
| PH013_SYNC001–002 | SyncSummary default values |

---

## Files Modified

| File | Change |
|------|--------|
| `src/BMPharma.CHIFA/Interfaces/IChifaIntegrationFacade.cs` | Added monitoring properties to `DashboardOverview` |
| `src/BMPharma.CHIFA/Services/StatusSynchronizer.cs` | Added `CountInvoicesTodayAsync()` |
| `src/BMPharma.CHIFA/Services/ChifaIntegrationFacade.cs` | Enriched `GetDashboardOverviewAsync()` |
| `src/BMPharma.UI/ViewModels/ChifaDashboardViewModel.cs` | Full rewrite: real data, monitoring, auto-refresh |
| `src/BMPharma.UI/ViewModels/ChifaInvoicePreparationViewModel.cs` | Preview step, ReadOnly guard, better errors |

## Files Created

| File | Purpose |
|------|---------|
| `src/BMPharma.CHIFA/Services/ChifaExceptionMapper.cs` | Exception → user-friendly French message mapping |
| `tests/BMPharma.CHIFA.Tests/ChifaPhase013IntegrationTests.cs` | 24 test cases for Phase 013 changes |
| `BM-PHASE-013-UI-INTEGRATION.md` | This documentation |

## Non-Functional Constraints

- ✅ Aucun changement de contrat public (DashboardOverview DTO enriched, interface unchanged)
- ✅ Aucun refactoring majeur (additive changes only)
- ✅ Tous les changements sont isolés dans les couches UI et CHIFA
- ✅ Tests automatisés pour chaque nouveau comportement
