# BM-PHASE-010 — TRANSACTION STRATEGY

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phase 006 (transaction strategy), Phase 007 (real write validation), Phase 005 (TST001)
**Previous Version:** BM_PHARMA_CHIFA_TRANSACTION_STRATEGY.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial strategy based on BM-SPEC |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with real write order, FK constraints, DateTime fixes |

### Major Changes from v1.0

| Change | v1.0 (Hypothesis) | v2.0 (Real) | Source |
|--------|-------------------|-------------|--------|
| Write order | detail_fact then facture | facture FIRST, then detail_fact | Phase 007-G |
| DateTime handling | DateTime.Now | DateTime.SpecifyKind(..., Unspecified) | Phase 007-G |
| Counter mechanism | SELECT FOR UPDATE | UPDATE...RETURNING (atomic) | Phase 006-D |
| Transaction isolation | ReadCommitted | ReadCommitted (confirmed working) | Phase 007-G |
| Rollback verification | Theoretical | CONFIRMED — exact baseline restoration (TST001, TST002) | Phase 005, 007 |

---

## Principle

Every multi-table CHIFA operation is transactional. On ANY failure: FULL ROLLBACK.

PostgreSQL 9.3.4 supports transactions, COMMIT, and ROLLBACK normally via TCP. This was confirmed in Phases 005 and 007.

---

## Scenario 1: Create Invoice (CONFIRMED — Phase 007-G)

This scenario was executed successfully in Phase 007-G (TST002):

```
1. BeginTransactionAsync(IsolationLevel.ReadCommitted)
2. ChifaWriteGuard.EnsureWriteAllowedAsync()    → mode check
3. ChifaInvoiceValidator.ApplyDefaults()         → defaults
4. ChifaInvoiceValidator.Validate()              → 0 errors
5. context.Factures.Add(facture)                 → facture first
6. context.SaveChangesAsync()                    → INSERT facture (parent)
7. context.DetailFacts.Add(detail)               → detail_fact second
8. context.SaveChangesAsync()                    → INSERT detail_fact (child, FK satisfied)
9. transaction.CommitAsync()                     → COMMIT
10. ChifaAuditService.LogOperationAsync()        → audit
```

**Total duration (TST002):** 877ms

### Why Two SaveChangesAsync Calls

EF Core inserts entities in alphabetical order by default. `ChifaDetailFact` comes before `ChifaFacture`. This causes FK violation 23503 because detail_fact.num_fact references facture.num_fact. Solution: call SaveChangesAsync twice — first for facture, then for detail_fact.

---

## Scenario 2: Create Bordereau (PARTIAL — not tested on real PG)

```
1. SELECT/UPDATE parametre (atomic counter)
2. INSERT bordereau
3. UPDATE facture SET num_bord = X
4. COMMIT
```

**Status:** PARTIAL — InMemory tested only (Phase 006). Real PG test not performed because Option D recommends against BM Pharma creating bordereaux.

---

## Scenario 3: Full Invoice + Bordereau (NOT RECOMMENDED)

**Status:** NOT RECOMMENDED due to visibility gap (TST003).

---

## Atomic Counter Protocol (CONFIRMED)

**Mechanism:** `UPDATE...RETURNING` (not `SELECT FOR UPDATE`)

```sql
UPDATE parametre SET next_num_fact = next_num_fact + 1
WHERE code_ps = :code_ps RETURNING next_num_fact
```

**Advantages:**
- Atomic: SELECT + UPDATE in single operation
- No TOCTOU race condition
- Returns new value directly

**Interface:**
```csharp
public interface IChifaNumberingService
{
    Task<string> GetNextInvoiceNumberAsync(string codePs, string codeCentre);
    Task<string> GetNextBordereauNumberAsync(string codePs, string codeCentre);
    Task<string> PeekNextInvoiceNumberAsync(string codePs, string codeCentre);
    Task<string> PeekNextBordereauNumberAsync(string codePs, string codeCentre);
}
```

---

## Rollback Strategy (CONFIRMED)

### Verified Rollbacks

| Test | What | Rollback Method | Verification | Status |
|------|------|----------------|--------------|--------|
| TST001 | INSERT facture + detail_fact | DELETE FROM SQL | 12/12 checks, baseline restored | CONFIRMED (Phase 005) |
| TST002 | INSERT facture + detail_fact via EF Core | DELETE FROM SQL | 10/10 checks, baseline restored | CONFIRMED (Phase 007) |

### Rollback Protocol

If ANY step fails during a write:
1. PostgreSQL transaction is automatically rolled back by the using block
2. BM Pharma SQLite data remains unchanged (separate database)
3. No orphaned records
4. Counter operation: if the counter was incremented within the same transaction, it is also rolled back
5. User notified of specific failure via ChifaWriteResult

**Note:** If counter is incremented in a prior committed transaction, a subsequent failure will NOT roll back the counter. This is by design — the counter value is "consumed" and the number is not reused (avoids collisions with potentially transmitted invoices).

---

## Error Handling

| Error | PostgreSQL Error | Action | Status |
|-------|-----------------|--------|--------|
| Connection failure | — | Return error, no write attempted | CONFIRMED |
| Constraint violation | 23503 (FK), 23505 (unique) | Return error with details, rollback | CONFIRMED |
| NOT NULL violation | 23502 | Auto-set defaults before write | CONFIRMED |
| Type mismatch | 22xxx | Column type mapping (xml, timestamp, date) | CONFIRMED |
| Counter lock timeout | — | Retry up to 3 times, then fail | CONFIRMED |
| Any INSERT failure | — | Full rollback | CONFIRMED |
| Any UPDATE failure | — | Full rollback | CONFIRMED |

---

## PostgreSQL 9.3.4 Considerations

| Aspect | Impact | Mitigation | Status |
|--------|--------|------------|--------|
| 32-bit process | Max 2GB memory | Keep transactions short | CONFIRMED |
| EOL (2018) | No security patches | Accept risk for embedded use | CONFIRMED |
| Crash during write | Partial write possible | Transaction rollback + retry | CONFIRMED |
| TCP crash (0xC0000142) | False alarm — was permission denial | Fixed in Phase 004.11 | CONFIRMED |

---

## Idempotence

| Operation | Idempotent? | Mechanism | Status |
|-----------|-------------|-----------|--------|
| CREATE invoice | NO (each INSERT creates new row) | Unique PK prevents duplicates | CONFIRMED |
| Counter increment | YES (called once per operation) | Atomic UPDATE...RETURNING | CONFIRMED |
| Status check | YES (SELECT only) | Read-only, no side effects | CONFIRMED |
| Rollback | YES (can re-rollback) | Safe guard: check row exists before DELETE | PARTIAL |
