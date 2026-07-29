# BM-PHASE-012-C — Exception Audit

## Exception Hierarchy

```
ChifaConnectionException
  ├── Origin: IntegrationService (CHIFA unreachable, DB timeout)
  ├── Propagation: Caught by Facade → returns NotAvailable
  ├── Retry: CircuitBreaker (3 failures → open 30s)
  └── Rollback: N/A — no writes occurred

ChifaValidationException
  ├── Origin: ChifaInvoiceValidator / ChifaBordereauValidator
  ├── Propagation: Caught by Facade → returns ValidationFailed
  ├── Retry: No — validation must be fixed by user
  └── Rollback: N/A — no writes occurred

ChifaWriteException
  ├── Origin: ChifaPostgresInvoiceService (EF Core SaveChanges)
  ├── Propagation: Caught by Facade → returns WriteFailed
  ├── Retry: Yes — callers may retry (idempotent with InvoiceExists check)
  └── Rollback: Automatic via DbContext transaction (SaveChanges failure)

ChifaConcurrencyException
  ├── Origin: EF Core DbUpdateConcurrencyException
  ├── Propagation: Caught by Facade → returns WriteFailed
  ├── Retry: Yes — recommended with fresh data
  └── Rollback: Automatic via DbContext transaction

ChifaSynchronizationException
  ├── Origin: InvoiceSynchronizer / BordereauSynchronizer
  ├── Propagation: Caught by Facade → returns CollectionResult with errors
  ├── Retry: Yes — synchronize is designed to be retried
  └── Rollback: N/A — sync is read-only

ChifaVisibilityException
  ├── Origin: StatusSynchronizer (invoice not visible after write)
  ├── Propagation: Caught by Facade → returns NotFound
  ├── Retry: Yes — delay and retry (PG replication lag)
  └── Rollback: N/A — visibility check is read-only

ChifaWriteBlockedException
  ├── Origin: ChifaWriteGuard (ReadOnly mode)
  ├── Propagation: Thrown directly to caller
  ├── Retry: No — mode must be changed
  └── Rollback: N/A — no writes attempted
```

## Exception Propagation Diagram

```
ViewModel
  └── IChifaIntegrationFacade
        └── try/catch wrapping every method
              ├── ChifaConnectionException → FacadeOperationStatus.NotAvailable
              ├── ChifaValidationException → FacadeOperationStatus.ValidationFailed
              ├── ChifaWriteException → FacadeOperationStatus.WriteFailed
              ├── ChifaConcurrencyException → FacadeOperationStatus.WriteFailed
              ├── ChifaSynchronizationException → logged, included in Errors list
              ├── ChifaVisibilityException → FacadeOperationStatus.NotFound
              ├── ChifaWriteBlockedException → propagated (no facade catch)
              └── Exception (generic) → FacadeOperationStatus.WriteFailed
```

## Circuit Breaker Integration

```
StatusEngine.EvaluateAsync()
  ├── ChifaCircuitBreaker.IsOpen("chifa")?
  │     YES → return TechnicalStatus.Disconnected (no call to CHIFA)
  │     NO → proceed
  ├── GetHealthStatusAsync()
  │     SUCCESS → RecordSuccess("chifa")
  │     FAILURE → RecordFailure("chifa")
  └── Return ChifaStatusSnapshot
```

## Exception Logging

All exceptions logged via `ILogger`:
- `LogError` in Facade methods
- `LogWarning` in health check / token / signing checks
- `LogWarning` in audit service failures (non-critical)

## Unhandled Exception Risks

| Scenario | Risk | Mitigation |
|---|---|---|
| `ChifaWriteBlockedException` not caught in Facade | Propagates to ViewModel | ViewModel has generic catch |
| Npgsql connection pool exhaustion | Deadlock | CircuitBreaker opens, limits retries |
| DbUpdateException with inner PG error | SQL details leaked | Logged, user sees generic message |
| CancellationToken during write | Partial write | DbContext transaction rolls back |
