# BM-PHASE-011-03-DESIGN-STATUS-ENGINE — Status Engine Design

## Three Axes

### TechnicalStatus
| Value | Meaning |
|---|---|
| `Unknown` | Not yet evaluated |
| `Connected` | CHIFA online + DB connected |
| `Degraded` | Partial connectivity (online but DB down, or vice versa) |
| `Disconnected` | Neither CHIFA nor DB available |

### BusinessStatus
| Value | Meaning |
|---|---|
| `Unknown` | Not yet determined |
| `Draft` | In BM Pharma, not yet prepared |
| `Prepared` | ChifaInvoiceRequest built, validation pending |
| `Validated` | Validation passed |
| `Persisted` | Written to CHIFA facture table |
| `Failed` | Operation failed |

### VisibilityStatus
| Value | Meaning |
|---|---|
| `Unknown` | Not yet checked |
| `VisibleInFacture` | Present in facture table, no bordereau assigned |
| `VisibleInBordereau` | Assigned to a bordereau |
| `NotVisible` | Not found in CHIFA |

## ChifaStatusSnapshot

```csharp
public class ChifaStatusSnapshot
{
    public TechnicalStatus Technical { get; set; }
    public BusinessStatus Business { get; set; }
    public VisibilityStatus Visibility { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public long DurationMs { get; set; }

    public bool IsReady => Technical == TechnicalStatus.Connected
        && Business == BusinessStatus.Persisted
        && Visibility == VisibilityStatus.VisibleInFacture;
}
```

## StatusEngine Implementation

`StatusEngine.EvaluateAsync()`:
1. Checks `ChifaCircuitBreaker` — if open, returns `Disconnected` immediately
2. Calls `IChifaIntegrationService.GetHealthStatusAsync()`
3. Maps `health.IsOnline + health.IsDatabaseConnected` to `TechnicalStatus`
4. Records success/failure on circuit breaker
5. Returns `ChifaStatusSnapshot` with `DurationMs`

## Specialized Snapshots

- `ChifaInvoiceStatusSnapshot`: NumFact, BusinessStatus, VisibilityStatus, Etat, NumBord, MontFact, ExistsInChifa
- `ChifaBordereauStatusSnapshot`: NumBord, Etat, ExistsInChifa, InvoiceCount
- `ChifaSyncSummary`: InvoicesFound/Updated, BordereauxFound/Updated, Errors
