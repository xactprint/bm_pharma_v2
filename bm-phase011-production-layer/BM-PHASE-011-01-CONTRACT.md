# BM-PHASE-011-01-CONTRACT — Phase 011: Production Integration Layer

## Objective

Transform all Phase 010 validated results into a single, production-grade CHIFA integration layer with:

1. **`IChifaIntegrationFacade`** — Single entry point replacing 5+ direct service injections in ViewModels
2. **Status Engine** — TechnicalStatus (Connected/Degraded/Disconnected), BusinessStatus (Draft→Persisted), VisibilityStatus (VisibleInFacture/VisibleInBordereau/NotVisible)
3. **Synchronizers** — InvoiceSynchronizer, BordereauSynchronizer, StatusSynchronizer
4. **Monitoring** — HealthCheck, CircuitBreaker, Metrics, CorrelationContext, Retry/Timeout infrastructure
5. **Exception Hierarchy** — 7 explicit exception types (Connection, Validation, Write, Concurrency, Synchronization, Visibility, generic)
6. **~90 new tests** — 593+ total, 0 regression, 0 warning
7. **10 documentation deliverables**

## Absolute Constraints

- No experimentation, reverse engineering, or CHIFA-OFFICINE modification
- Option D (Hybrid) architecture: BM writes only `facture` + `detail_fact`
- CHIFA-OFFICINE handles bordereau creation, signing, closure, CNAS transmission
- ViewModels must depend ONLY on `IChifaIntegrationFacade` (not on 5+ low-level services)

## Architecture

```
IChifaIntegrationFacade ← ChifaIntegrationFacade
  ├── StatusEngine (TechnicalStatus, BusinessStatus, VisibilityStatus)
  ├── InvoiceSynchronizer / BordereauSynchronizer / StatusSynchronizer
  ├── ChifaMonitoringService (HealthCheck, CircuitBreaker, Metrics)
  ├── CorrelationContext (AsyncLocal per-operation)
  └── ChifaExceptions (7 types)
```

## Files Created

- `src/BMPharma.CHIFA/Interfaces/IChifaIntegrationFacade.cs`
- `src/BMPharma.CHIFA/Services/ChifaIntegrationFacade.cs`
- `src/BMPharma.CHIFA/Services/StatusEngine.cs` + `StatusEngineModels.cs`
- `src/BMPharma.CHIFA/Services/InvoiceSynchronizer.cs`
- `src/BMPharma.CHIFA/Services/BordereauSynchronizer.cs`
- `src/BMPharma.CHIFA/Services/StatusSynchronizer.cs`
- `src/BMPharma.CHIFA/Services/ChifaCircuitBreaker.cs`
- `src/BMPharma.CHIFA/Services/ChifaMetricsService.cs`
- `src/BMPharma.CHIFA/Services/ChifaHealthCheckService.cs`
- `src/BMPharma.CHIFA/Services/ChifaMonitoringService.cs`
- `src/BMPharma.CHIFA/Services/CorrelationContext.cs`
- `src/BMPharma.CHIFA/Services/ChifaExceptions.cs`

## Files Modified

- `src/BMPharma.CHIFA/DependencyInjection.cs` — Register facade, engine, synchronizers, monitoring
- `src/BMPharma.UI/ViewModels/ChifaDashboardViewModel.cs` — Inject IChifaIntegrationFacade only
- `src/BMPharma.UI/ViewModels/ChifaInvoicePreparationViewModel.cs` — Inject IChifaIntegrationFacade only
- `src/BMPharma.UI/ViewModels/ChifaBordereauStatusViewModel.cs` — Inject IChifaIntegrationFacade only

## Verification

- 593+ tests total (509 existing + 84 new)
- 0 test regression (all existing tests preserved)
- All ViewModels compile with new DI
- All monitors, synchronizers, circuit breaker tested independently
