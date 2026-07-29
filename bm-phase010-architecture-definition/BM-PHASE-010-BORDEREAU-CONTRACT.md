# BM-PHASE-010 — BORDEREAU CONTRACT

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE — PARTIAL (visibility gap documented)
**Source of Truth:** Phase 009 (reverse engineering), Phase 009-C (memory scan), Phase 008-A (lifecycle audit)
**Previous Version:** BM_PHARMA_CHIFA_BORDEREAU_CONTRACT.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial contract based on BM-SPEC-031 |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with bordereau lifecycle discovery (Phases 008, 009, 009-C) |

### Major Changes from v1.0

| Change | v1.0 (Hypothesis) | v2.0 (Real) | Source |
|--------|-------------------|-------------|--------|
| detail_bord table | Assumed PostgreSQL table | CONFIRMED as .NET DataTable | Phase 009-C |
| Visibility | Assumed visible if written | PARTIAL — may be invisible in Visualiser Bordereau | TST003 |
| BM Pharma creates bordereau | Assumed possible | POSSIBLE but NOT RECOMMENDED | Phase 009 |
| Signing | Delegated to CHIFA | CONFIRMED — Identiv uTrust 3512 + p7sign.dll | Phase 008-A |
| Cloture | Delegated to CHIFA | CONFIRMED — cloturerbord() PG function | Phase 008-A |
| CNAS transmission | Delegated to CHIFA | CONFIRMED — FTP to 41.111.149.250:21 | Phase 008-A |
| SQL queries | Unknown | 4 families CONFIRMED via memory scan | Phase 009-C |

---

## Purpose

Defines how BM Pharma interacts with CHIFA bordereaux. Due to the confirmed visibility gap (TST003), the recommended architecture (Option D) has BM Pharma creating only facture/detail_fact and letting CHIFA handle bordereau creation natively.

---

## Visibility Gap (Critical)

> **DATABASE WRITE ≠ CHIFA-OFFICINE VISIBILITY**

| Fact | Detail | Status |
|------|--------|--------|
| facture visible in Consultation Facture | TST001 confirmed visible | CONFIRMED (Phase 008) |
| bordereau visible in Visualiser Bordereau | TST003 not consistently visible | PARTIAL (Phase 009) |
| detail_bord = PostgreSQL table | FALSE — not a PG table | FALSE (Phase 009-C) |
| detail_bord = .NET DataTable | CONFIRMED in CHIFA process memory | CONFIRMED (Phase 009-C) |
| detail_bord filling algorithm | Not fully determined — runtime SQL parameters unknown | UNKNOWN |
| Root cause of TST003 invisibility | 6 hypotheses, none confirmed | UNKNOWN (Phase 009-C) |

### Implications

BM Pharma MUST NOT assume that a bordereau written to PostgreSQL will be visible in CHIFA-OFFICINE's "Visualiser Bordereau" interface. The bordereau listing query in CHIFA uses a compiled SQL constant loaded at startup, with runtime filtering conditions that are not fully determined.

### Recommended Approach (Option D)

Do not create bordereaux from BM Pharma. Instead:
1. Write facture + detail_fact to PostgreSQL
2. Guide user to open CHIFA-OFFICINE
3. User creates bordereau natively in CHIFA via Vente Chifa → FBordereau
4. CHIFA handles signing, closure, transmission
5. BM Pharma monitors status via SELECT on facture.etat, bordereau.etat, signature table

---

## Bordereau Lifecycle in CHIFA-OFFICINE

```
Vente Chifa (Invoice Entry)
    ↓
FBordereau (Bordereau Form)
    ↓
detail_bord DataTable population (runtime algorithm, partially unknown)
    ↓
Signature (Identiv uTrust 3512 + p7sign.dll → PKCS#7)
    ↓
Cloture (cloturerbord() PostgreSQL function)
    ↓
Transmission (FTP to 41.111.149.250:21)
    ↓
CNAS (National Healthcare)
```

---

## 4 SQL Query Families (CONFIRMED via Phase 009-C Memory Scan)

### Family 1: Bordereau Listing
```sql
SELECT b.num_bord, ... FROM bordereau b, facture f
WHERE ... GROUP BY ... ORDER BY num_ordre DESC
```
**Status:** CONFIRMED — compiled constant, loaded at startup
**Variants:** Open/closed bordereaux (different WHERE conditions)

### Family 2: detail_bord Population
```sql
SELECT * FROM facture WHERE num_bord='.' AND CODE_CENTRE='.' AND etat='S' ORDER BY TP, NUM_ASSURE, NUM_FACT
```
**Status:** CONFIRMED — parameters filled at runtime
**Note:** This is the SQL that populates the .NET DataTable `detail_bord`
**Unknown:** Exact runtime parameter values, post-SQL filtering

### Family 3: Detail Medicaments
```sql
detail_fact LEFT OUTER JOIN medicament ... WHERE num_fact='.'
```
**Status:** CONFIRMED

### Family 4: Consultative Facture
```sql
beneficiaire LEFT JOIN ... bordereau LEFT JOIN ...
ORDER BY date_fact DESC LIMIT 1000
```
**Status:** CONFIRMED

---

## BM Pharma Bordereau Operations (Limited)

### What BM Pharma CAN do

| Operation | Method | Verified | Status |
|-----------|--------|----------|--------|
| Get next bordereau number | ChifaNumberingService.GetNextBordereauNumberAsync | Phase 006 | CONFIRMED |
| INSERT bordereau row | ChifaPostgresBordereauService | Phase 006 (InMemory) | PARTIAL |
| UPDATE facture.num_bord | EF Core | Phase 006 (InMemory) | PARTIAL |
| Monitor bordereau status | SELECT bordereau WHERE num_bord = X | Architecture | CONFIRMED |
| Monitor invoice signature | SELECT signature IS NOT NULL | Architecture | CONFIRMED |
| Monitor closure status | SELECT bordereau.etat = 'C' | Architecture | CONFIRMED |
| Monitor transmission | SELECT date_depot_ftp IS NOT NULL | Architecture | CONFIRMED |

### What BM Pharma CANNOT do (requires CHIFA-OFFICINE)

| Operation | Requirement | Status |
|-----------|------------|--------|
| Sign bordereau | Identiv uTrust 3512 + p7sign.dll | NOT SUPPORTED |
| Close bordereau | cloturerbord() PostgreSQL function | NOT SUPPORTED |
| Transmit to CNAS | FTP to 41.111.149.250:21 | NOT SUPPORTED |
| Populate detail_bord | Unknown runtime algorithm | UNKNOWN |
| Guarantee visibility in Visualiser Bordereau | Unknown rendering logic | UNKNOWN |

---

## Counter Management

| Counter | Table.Column | Current Value | Mechanism | Status |
|---------|-------------|--------------|-----------|--------|
| Invoice | parametre.next_num_fact | 1 | UPDATE...RETURNING (atomic) | CONFIRMED |
| Bordereau | parametre.next_num_bord | 215 | UPDATE...RETURNING (atomic) | CONFIRMED |

### Atomic Counter Protocol (CONFIRMED)

```sql
UPDATE parametre SET next_num_bord = next_num_bord + 1
WHERE code_ps = :code_ps RETURNING next_num_bord
```

### Counter Formatting

| Counter | Format | Example | Verified |
|---------|--------|---------|----------|
| num_fact | varchar(8), zero-padded | `00000001` | CONFIRMED |
| num_bord | varchar(6), zero-padded | `000215` | CONFIRMED |

---

## Collision Prevention

| Mechanism | Detail | Status |
|-----------|--------|--------|
| Atomic UPDATE...RETURNING | No TOCTOU race condition | CONFIRMED |
| Unique index on bordereau.num_bord | idx_bordereau_num_bord (UNIQUE) | CONFIRMED |
| FK constraint facture → bordereau | fk_facture_bordereau | CONFIRMED |
| Transaction rollback | Auto-rollback on failure | CONFIRMED |

---

## Risk Register (Bordereau-Specific)

| Risk | Probability | Impact | Mitigation | Status |
|------|------------|--------|------------|--------|
| Bordereau invisible in CHIFA | HIGH (TST003 confirmed) | MEDIUM | Option D: let CHIFA create bordereaux natively | CONFIRMED |
| detail_bord algorithm unknown | CERTAIN | MEDIUM | Cannot replicate; CHIFA handles it | CONFIRMED |
| Counter collision | LOW | HIGH | Atomic UPDATE...RETURNING | CONFIRMED |
| FK violation on num_bord assignment | LOW | MEDIUM | Transaction rollback | CONFIRMED |
