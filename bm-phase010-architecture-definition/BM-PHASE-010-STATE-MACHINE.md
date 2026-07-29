# BM-PHASE-010 — STATE MACHINE

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phase 003 (workflow state machine), Phase 008 (lifecycle audit), Phase 009 (visibility analysis)
**Previous Version:** BM_PHARMA_CHIFA_STATE_MACHINE.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial state machine based on BM-SPEC |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with visibility gap, detail_bord DataTable finding, lifecycle audit |

### Major Changes from v1.0

| Change | v1.0 (Hypothesis) | v2.0 (Real) | Source |
|--------|-------------------|-------------|--------|
| CreatedInDatabase = CHIFAVisible | Assumed equal | CONFIRMED as distinct states | Phase 008, TST003 |
| Bordereau state visibility | Assumed always visible | PARTIAL — may be invisible in Visualiser Bordereau | Phase 009 |
| detail_bord state | Assumed PG table | CORRECTED — .NET DataTable, not a persistence boundary | Phase 009-C |
| Persisted vs Visible distinction | Absent | ADDED — critical architectural invariant | Phase 010 |

---

## Fundamental Distinction

> **PERSISTED ≠ VISIBLE IN CHIFA**

This is the single most important architectural invariant. BM Pharma must NEVER treat a database write as equivalent to CHIFA-OFFICINE visibility.

---

## Invoice States (Reconciled)

```
[BM Pharma Local]
    → Draft (SQLite, not yet in CHIFA)
    → Validated (passes all CHIFA constraints)
    → PreparedForChifa (mapped, ready for write)

[CHIFA PostgreSQL]
    → Persisted (written to CHIFA PG — CONFIRMED via Phase 007)

[CHIFA-OFFICINE]
    → VisibleInChifa (confirmed visible in CHIFA Consultation Facture — CONFIRMED via Phase 008)
    → Signed (PKCS#7 signature applied — NOT SUPPORTED by BM Pharma)
    → BordereauAssigned (linked to bordereau — PARTIAL visibility)
    → BordereauClosed (bordereau closed via cloturerbord() — NOT SUPPORTED)
    → Transmitted (sent to CNAS via FTP — NOT SUPPORTED)
    → Reimbursed (CNAS reimbursement completed — NOT SUPPORTED)

[Error States]
    → Failed (error occurred)
    → Rejected (rejected by CHIFA/CNAS)
    → RollbackRequired (data inconsistency detected)
    → Cancelled (user cancelled)
```

### State Transition Diagram

```
Draft ──→ Validated ──→ PreparedForChifa ──→ Persisted ──→ VisibleInChifa ──→ Signed ──→ BordereauAssigned ──→ BordereauClosed ──→ Transmitted ──→ Reimbursed
  ↑           ↑               ↑                    │                │              │              │                   │                   │
  │           │               │                    │                │              │              │                   │                   │
  └─── Cancelled              │                    ▼                ▼              ▼              ▼                   ▼                   ▼
                              └─── Failed ──── Rejected ───── RollbackRequired ──── (all terminal errors)
```

---

## Bordereau States (CHIFA-OFFICINE controlled)

```
Draft → Preparing → Created → InvoicesAttached → AwaitingSignature →
PartiallySigned → ReadyForClosure → AwaitingClosure → Closed →
AwaitingTransmission → Transmitted → Completed

Error states: Error, SyncError, SignatureError, ClosureError, TransmissionError
```

---

## State Definitions

| State | PG Row Exists | CHIFA UI Visible | Signed | Transmitted | BM Pharma Can Verify |
|-------|--------------|-----------------|--------|-------------|---------------------|
| Draft | No | No | No | No | CONFIRMED |
| Validated | No | No | No | No | CONFIRMED |
| PreparedForChifa | No | No | No | No | CONFIRMED |
| Persisted | **Yes** | **UNKNOWN** | No | No | CONFIRMED |
| VisibleInChifa | Yes | **Yes** | No | No | PARTIAL (TST001 confirmed) |
| Signed | Yes | Yes | **Yes** | No | NOT SUPPORTED (SELECT signature only) |
| BordereauAssigned | Yes (bordereau + facture.num_bord) | PARTIAL | Yes | No | PARTIAL |
| BordereauClosed | Yes (bordereau.etat='C') | Yes | Yes | No | CONFIRMED (SELECT etat) |
| Transmitted | Yes (date_depot_ftp set) | Yes | Yes | Yes | CONFIRMED (SELECT date_depot_ftp) |
| Reimbursed | Yes | Yes | Yes | Yes | NOT SUPPORTED |

---

## State Transition Rules

| From | To | Actor | Action | Status |
|------|-----|-------|--------|--------|
| Draft | Validated | BM Pharma | Invoice validated locally | CONFIRMED |
| Validated | PreparedForChifa | BM Pharma | Data prepared for CHIFA | CONFIRMED |
| PreparedForChifa | Persisted | BM Pharma | INSERT facture + detail_fact via EF Core | CONFIRMED |
| Persisted | VisibleInChifa | CHIFA-OFFICINE | User opens CHIFA Consultation Facture | PARTIAL |
| VisibleInChifa | Signed | CHIFA-OFFICINE | PKCS#7 signing via token | NOT SUPPORTED |
| Signed | BordereauAssigned | CHIFA-OFFICINE | User creates bordereau in CHIFA | NOT SUPPORTED |
| BordereauAssigned | BordereauClosed | CHIFA-OFFICINE | cloturerbord() executed | NOT SUPPORTED |
| BordereauClosed | Transmitted | CHIFA-OFFICINE | FTP to CNAS (41.111.149.250) | NOT SUPPORTED |
| Transmitted | Reimbursed | CNAS | CNAS processes payment | NOT SUPPORTED |
| Any | Failed | BM Pharma | Error during operation | CONFIRMED |
| Any | Rejected | CHIFA/CNAS | Data rejected | PARTIAL |
| Persisted | RollbackRequired | BM Pharma | Inconsistency detected, rollback needed | CONFIRMED |
| Any | Cancelled | User | User aborts | CONFIRMED |

---

## Critical State: Persisted

**Persisted = TRUE** means:
- Row exists in facture table (confirmed via SELECT)
- Row exists in detail_fact table (confirmed via SELECT)
- Counter may or may not have been incremented (depends on transaction)

**Persisted = TRUE does NOT mean:**
- Visible in CHIFA Consultation Facture (UNKNOWN until verified)
- Processed by CHIFA-OFFICINE (UNKNOWN)
- Signed (FALSE — requires token)
- Part of any bordereau (FALSE until assigned)

---

## Human Action Points

| State | User Action Required | Application | Status |
|-------|---------------------|-------------|--------|
| VisibleInChifa | User opens CHIFA and navigates to Consultation Facture | CHIFA-OFFICINE | PARTIAL |
| Sign | User inserts professional card, enters PIN | CHIFA-OFFICINE | NOT SUPPORTED |
| BordereauAssigned | User creates bordereau in CHIFA, attaches invoices | CHIFA-OFFICINE | NOT SUPPORTED |
| BordereauClosed | User triggers cloture in CHIFA | CHIFA-OFFICINE | NOT SUPPORTED |
| Transmitted | User initiates CNAS transmission in CHIFA | CHIFA-OFFICINE | NOT SUPPORTED |

---

## BM Pharma Status Verification

BM Pharma can verify these states via READ-ONLY queries:

| State | SQL Check | Status |
|-------|-----------|--------|
| Persisted | `SELECT COUNT(*) FROM facture WHERE num_fact = 'X'` | CONFIRMED |
| Signed | `SELECT signature IS NOT NULL FROM facture WHERE num_fact = 'X'` | CONFIRMED |
| BordereauClosed | `SELECT etat FROM bordereau WHERE num_bord = 'X'` | CONFIRMED |
| Transmitted | `SELECT date_depot_ftp FROM bordereau WHERE num_bord = 'X'` | CONFIRMED |
| VisibleInChifa | No direct SQL check — must verify via CHIFA UI | UNKNOWN |
