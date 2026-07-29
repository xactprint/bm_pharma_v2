# BM-PHASE-011-05-DESIGN-ERRORS — Exception Hierarchy

## Exception Types

```
ChifaConnectionException
  Server, Message, InnerException
  → CHIFA server unreachable, DB connection failure

ChifaValidationException
  Errors (IReadOnlyList<ChifaValidationError>)
  → Invoice validation failures, missing fields

ChifaWriteException
  EntityType, EntityKey, Message, InnerException
  → PG write failure, FK violation, column mismatch

ChifaConcurrencyException
  Resource, Message, InnerException
  → Concurrent modification, row version conflict

ChifaSynchronizationException
  Source, Message, InnerException
  → Sync failure, data inconsistency

ChifaVisibilityException
  EntityType, EntityKey, Message, InnerException
  → Invoice not visible in CHIFA after write

ChifaWorkflowException (not yet created)
  → State machine transition failure (future)
```

## Error Handling Pattern

All operations in `ChifaIntegrationFacade` follow this pattern:

```csharp
var sw = Stopwatch.StartNew();
try
{
    _correlation.GetOrCreate();
    // ... business logic ...
    return FacadeResult { Status = Success, Data = ..., DurationMs = sw.ElapsedMilliseconds };
}
catch (ChifaConnectionException ex)
{
    _logger.LogError(ex, "...");
    _circuitBreaker.RecordFailure("chifa");
    return FacadeResult { Status = NotAvailable, ErrorMessage = ex.Message, DurationMs = sw.ElapsedMilliseconds };
}
catch (Exception ex)
{
    _logger.LogError(ex, "...");
    return FacadeResult { Status = WriteFailed, ErrorMessage = ex.Message, DurationMs = sw.ElapsedMilliseconds };
}
```

## ChifaValidationError

```csharp
public class ChifaValidationError
{
    public string Code { get; set; }      // e.g. "REQUIRED", "INVALID", "RANGE"
    public string Field { get; set; }     // e.g. "NumFact", "MontFact"
    public string Message { get; set; }   // Human-readable description
}
```

## FacadeOperationStatus Mapping

| Exception / Condition | FacadeOperationStatus |
|---|---|
| Validation failure | `ValidationFailed` |
| ReadOnly mode | `NotAvailable` |
| Invoice already exists | `Conflict` |
| Invoice not found | `NotFound` |
| Write failure | `WriteFailed` |
| Connection failure | `WriteFailed` |
| Success | `Success` |
