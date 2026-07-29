# BM-PHASE-010 — ERROR HANDLING & RESILIENCE

**Version:** 1.0 (Phase 010 creation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phases 004.7 (disaster recovery), 005 (TST001 rollback), 007 (TST002 bugs + rollback), 009-C (SQL analysis)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-29 | BM Pharma | Initial resilience document based on real PG experience |

---

## Resilience Principle

PostgreSQL 9.3.4 is EOL, 32-bit, and crash-prone. BM Pharma must treat the CHIFA database as a **fragile dependency** and design all operations with failure in mind.

---

## Error Categories

| Category | Examples | Frequency | Status |
|----------|----------|-----------|--------|
| Connection failures | PG service not running, port wrong, auth failure | LOW (local) | CONFIRMED |
| Constraint violations | FK 23503, unique 23505, NOT NULL 23502 | LOW (validated data) | CONFIRMED |
| Type mismatches | xml vs varchar, timestamp without time zone vs DateTime | MEDIUM (fixed in Phase 007) | CONFIRMED |
| PG crashes | 0xC0000142 (permission, not crash — Phase 004.11) | LOW (false alarm) | CONFIRMED |
| Visibility errors | Data written but not visible in CHIFA UI | MEDIUM (TST003) | PARTIAL |
| Counter errors | Race condition on counter increment | LOW (atomic UPDATE...RETURNING) | CONFIRMED |
| Idempotency errors | Duplicate writes due to retry | LOW (PK unique constraint) | CONFIRMED |

---

## Error Handling Strategy

### 1. Connection Retry with Exponential Backoff

```csharp
public async Task<T> ExecuteWithRetry<T>(
    Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (NpgsqlException ex) when (i < maxRetries - 1)
        {
            Logger.LogWarning(
                "Connection attempt {Attempt} failed: {Message}",
                i + 1, ex.Message);
            await Task.Delay(1000 * (i + 1)); // 1s, 2s, 3s
        }
    }
    throw new InvalidOperationException(
        "CHIFA database unavailable after retries");
}
```

### 2. Connection Availability Check

```csharp
public bool IsChifaAvailable()
{
    try
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return true;
    }
    catch (NpgsqlException)
    {
        return false;
    }
}
```

### 3. Transaction Rollback (Automatic)

All multi-table operations use `BeginTransactionAsync`. On exception, the `using` block automatically rolls back:

```csharp
await using var transaction =
    await context.Database.BeginTransactionAsync(
        IsolationLevel.ReadCommitted);
try
{
    // ... writes ...
    await transaction.CommitAsync();
}
catch
{
    // transaction.DisposeAsync() → automatic rollback
    throw;
}
```

---

## PostgreSQL Error Codes

| Code | Meaning | Handling | Status |
|------|---------|----------|--------|
| 23503 | Foreign key violation | Check FK ordering (facture before detail_fact) | CONFIRMED |
| 23505 | Unique violation | PK collision — return error, do not retry | CONFIRMED |
| 23502 | NOT NULL violation | Auto-set defaults before write | CONFIRMED |
| 22xxx | Type mismatch | Fix HasColumnType mapping | CONFIRMED |
| 40001 | Serialization failure | Retry transaction | CONFIRMED |
| 08001 | Connection failure | Retry with backoff | CONFIRMED |

---

## Visibility Error Handling

Since DATABASE WRITE ≠ CHIFA VISIBILITY, BM Pharma must handle the case where data is persisted but not visible:

| Strategy | Detail | Status |
|----------|--------|--------|
| Never assume visibility | Separate "Persisted" and "VisibleInChifa" states | CONFIRMED |
| Polling verification | Regular SELECT to check if CHIFA updated status | CONFIRMED |
| User guidance | Instruct user to open CHIFA and refresh | CONFIRMED |
| Timeout detection | If not visible after N minutes, alert user | PARTIAL |

---

## Idempotence

| Operation | Idempotent? | Mechanism | Status |
|-----------|-------------|-----------|--------|
| CREATE invoice | NO | PK unique prevents double INSERT | CONFIRMED |
| READ invoice | YES | No side effects | CONFIRMED |
| UPDATE status | YES (BY CHIFA ONLY) | BM Pharma never UPDATEs | CONFIRMED |
| Counter increment | YES | Called once per transaction | CONFIRMED |
| Rollback | YES | Safe if no row exists | PARTIAL |

### Duplicate Prevention

- Primary keys (num_fact for facture, composite PK for detail_fact) prevent duplicate rows
- Unique index on bordereau.num_bord prevents duplicate bordereau numbers
- Atomic counter prevents two processes getting the same number
- If a counter is consumed but the transaction fails, the number is "lost" (not reused) — this is CORRECT BY DESIGN (avoids collisions with potentially transmitted data)

---

## Disaster Recovery

| Scenario | Action | Recovery Time | Status |
|----------|--------|---------------|--------|
| PG crash during write | Automatic transaction rollback | <1s | CONFIRMED |
| PG service down | Connection failure → retry → ReadOnly fallback | ~5s | CONFIRMED |
| Data inconsistency | Detect via SELECT → manual intervention | Hours | PARTIAL |
| Counter skipping | Detect gap → manual verification | Hours | PARTIAL |
| CHIFA-OFFICINE crash | BM Pharma continues in ReadOnly | Immediate | CONFIRMED |

---

## PostgreSQL 9.3.4 Specific Risks

| Risk | Detail | Mitigation | Status |
|------|--------|------------|--------|
| 32-bit memory limit | Max 2GB process memory | Keep transactions short | CONFIRMED |
| EOL (no patches) | Security vulnerabilities unpatched | Accept for embedded use | CONFIRMED |
| Embedded mode (postgres --single) | Bypasses auth | Not used by BM Pharma (TCP only) | CONFIRMED |
| TCP crash (0xC0000142) | Was permission denial (Phase 004.11) | Fixed — use `pharm` user | CONFIRMED |
| Npgsql 2.x vs 8.x on same machine | Separate processes, no conflict | Confirmed via IL analysis | CONFIRMED |

---

## Real-World Bugs Found (Phase 007-G)

| Bug | Symptom | Root Cause | Fix | Status |
|-----|---------|-----------|-----|--------|
| DateTime Kind mismatch | EF Core error: cannot write Local kind to timestamp without time zone | DateTime.Now returns Local, PG expects Unspecified | DateTime.SpecifyKind(value, Unspecified) | CONFIRMED FIXED |
| FK ordering | EF Core error: FK violation 23503 | EF Core saves alphabetically: ChifaDetailFact before ChifaFacture | Two SaveChangesAsync calls | CONFIRMED FIXED |
| Column type mismatches | EF Core error: cannot write varchar to xml column | EF Core maps xml columns as string by default | HasColumnType("xml") | CONFIRMED FIXED |

---

## Resilience Certitude Matrix

| Element | Status | Source |
|---------|--------|--------|
| Connection retry | CONFIRMED | Phase 004.7 |
| Transaction rollback | CONFIRMED | Phase 005, 007 |
| Automatic rollback on exception | CONFIRMED | Architecture |
| PK duplicate prevention | CONFIRMED | Phase 004.3 |
| Idempotent reads | CONFIRMED | Architecture |
| Non-idempotent writes | CONFIRMED | Must not retry blindly |
| Visibility error detection | PARTIAL | TST003 unresolved |
| PG crash recovery | CONFIRMED | Transaction rollback |
| FK violation prevention | CONFIRMED | Two SaveChangesAsync |
| DateTime kind handling | CONFIRMED FIXED | Phase 007-G |
| Column type handling | CONFIRMED FIXED | Phase 007-G |
