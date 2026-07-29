# BM-PHASE-012-D — Performance Review

## EF Core Query Analysis

### Read Queries (ChifaPostgreSqlContext)

| Query | Location | AsNoTracking? | Risk |
|---|---|---|---|
| `ChifaFactures.FirstOrDefaultAsync(f => f.NumFact == numFact)` | StatusSynchronizer.LoadInvoiceAsync | ✅ Yes | None |
| `ChifaFactures.OrderByDescending(f => f.DateFact).Take(100).ToListAsync()` | StatusSynchronizer.LoadAllInvoicesAsync | ✅ Yes | None |
| `ChifaFactures.Where(...).CountAsync()` | BordereauStatusService | ✅ Via context | Low |

### Write Queries (ChifaWriteDbContext)

| Query | Location | Tracking | Risk |
|---|---|---|---|
| `context.ChifaFactures.Add(facture)` | ChifaPostgresInvoiceService | Yes (default) | Normal |
| `context.ChifaDetailFacts.AddRange(details)` | ChifaPostgresInvoiceService | Yes (default) | Normal |
| `context.SaveChangesAsync()` | ChifaPostgresInvoiceService | Yes (default) | Normal |

## N+1 Query Risk

**None found.** All read operations use either single-row lookups or single batched `ToListAsync()`. No navigation properties are eagerly loaded or lazy-loaded in any CHIFA-related code.

## AsNoTracking Audit

| File | Method | Uses AsNoTracking | Status |
|---|---|---|---|
| StatusSynchronizer.cs | LoadInvoiceAsync | ✅ | OK |
| StatusSynchronizer.cs | LoadAllInvoicesAsync | ✅ | OK |
| ChifaPostgresInvoiceService.cs | Any query | ❌ (writes need tracking) | OK (write context) |
| ChifaPostgresBordereauService.cs | Any query | ❌ | Low risk — small dataset |

## Memory Allocation

| Component | Allocation Pattern | Risk |
|---|---|---|
| `ChifaMetricsService` | `ConcurrentBag<ChifaMetricPoint>` — grows unbounded | **LOW RISK** — metrics are diagnostic only, could be bounded |
| `BordereauStatusService` | `ConcurrentDictionary` for states — grows per session | **LOW** — session-scoped, cleared on restart |
| `ChifaInvoiceWorkflowService` | `ConcurrentBag<ChifaWorkflowAuditEntry>` — grows unbounded | **LOW** — audit entries limited to 20 in UI |
| `FakeChifaIntegrationProvider` | `ConcurrentDictionary` for invoices | **LOW** — ReadOnly mode only |
| `ChifaCircuitBreaker` | Fixed keys — no growth | None |

## Singleton/Scoped/Transient Audit

| Component | Lifetime | Risk |
|---|---|---|
| `ChifaIntegrationConfig` | Singleton | ✅ OK |
| `ChifaIntegrationModeProvider` | Singleton | ✅ OK |
| `FakeChifaIntegrationProvider` | Singleton | ✅ OK (stateless) |
| `CorrelationContext` | Singleton | ⚠️ AsyncLocal — OK for per-flow isolation |
| `ChifaCircuitBreaker` | Singleton | ✅ OK |
| `ChifaMetricsService` | Singleton | ✅ OK (diagnostic) |
| `ChifaIntegrationFacade` | Scoped | ✅ OK — holds scoped DbContexts |
| All DbContexts | Scoped | ✅ OK |
| All ViewModels | Transient | ✅ OK |

## Memory Leak Risks

| Risk | Severity | Mitigation |
|---|---|---|
| `ChifaMetricsService._points` unbounded | Low | Add cap (e.g., 10,000 entries) or periodic trim |
| `BordereauStatusService._states` per-session | Low | Session-scoped service — GC collected |
| `ChifaInvoiceWorkflowService._auditLog` unbounded | Low | UI shows max 20 entries |
| Timer-based health check in monitoring | Low | Timer disposed in `Dispose()` |

## Query Performance Recommendations

| Recommendation | Priority |
|---|---|
| Add `AsNoTracking()` to `ChifaPostgresBordereauService` read queries | Low |
| Cap `ChifaMetricsService` at 10,000 entries | Low |
| Add database index on `ChifaFactures.NumFact` (already exists — PK) | Already done |
| Add database index on `ChifaFactures.DateFact` (for ORDER BY) | Low — only 100 rows |
