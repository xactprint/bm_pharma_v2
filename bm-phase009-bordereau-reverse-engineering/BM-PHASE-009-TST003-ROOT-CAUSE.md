# BM-PHASE-009 — TST003 Root Cause Analysis

**Why Direct SQL INSERT Does Not Make a Bordereau Visible in Visualiser Bordereau**

> **Date:** 2026-07-28
> **Status:** COMPLETE
> **Classification:** Confidential — BM Pharma Internal

---

## Experiment Summary

| Property | Value |
|----------|-------|
| Test ID | TST003 |
| Objective | Validate theory that `detail_bord` is an in-memory DataTable, not a SQL view |
| Method | Create facture + detail_fact + bordereau via direct PostgreSQL INSERT |
| Expected (theory confirmed) | Invoice visible in Consultation Facture, bordereau invisible in Visualiser Bordereau |
| Expected (theory disproved) | Both visible — meaning `detail_bord` is a live SQL view |
| Actual result | Invoice visible, bordereau invisible |
| Rollback | Complete — 11/11 checks passed, baseline restored to facture=0, detail_fact=0, bordereau=0 |

---

## What Was Created

```sql
-- Bordereau 215
INSERT INTO bordereau (num_bord, code_centre, etat, date_ouverture, date_cloture)
VALUES ('215', '11600', NULL, NOW(), '1900-01-01');

-- Facture TST003
INSERT INTO facture (num_fact, ..., num_bord, ...)
VALUES ('TST003', ..., '215', ...);

-- detail_fact (4 lines with all critical non-NULL fields)
INSERT INTO detail_fact (num_fact, num_enr, qte, ppa, mont, ...)
VALUES ('TST003', ...);
```

All FK constraints satisfied. `next_num_fact` coordinated. `next_num_bord` coordinated.

---

## Observed Behavior

| Interface | Visibility | Observation |
|-----------|------------|-------------|
| Consultation Facture | **Visible** | TST003 appeared in facture list — direct SELECT from `facture` table |
| Visualiser Bordereau | **Invisible** | Bordereau 215 did not appear — `detail_bord` DataTable was not populated |

---

## Cause Analysis

### Cause Observable Immédiate (Observable Cause)

The bordereau was not visible because the `detail_bord` DataTable was not populated with the expected rows.

When `FormVisualiserBordereau` loads, it executes compiled SQL that populates `DataSet1.detail_bord`. The experiment data existed in PostgreSQL but was not loaded into the DataTable because:

1. The CHIFA form's load event did not execute the compiled SQL query
2. Or the compiled SQL query executed but the WHERE clause excluded the experiment data
3. Or the compiled SQL query depends on additional data that was not provided by the INSERTs

All three possibilities lead to the same observable result: empty DataTable → no bordereau displayed.

### Cause Racine Exacte (Exact Root Cause)

**NOT DETERMINED** at 100% certainty.

The exact root cause cannot be determined because:

1. **The compiled SQL query is unknown** — the WHERE clause may filter by conditions (etat, signature, date, quantity) that were not satisfied by the experiment data
2. **The fill pipeline is unknown** — CHIFA may require additional workflow steps (e.g., opening the invoice in CHIFA, validating, saving) before `detail_bord` is populated
3. **No SQL logs were captured** — the PostgreSQL `--forkcol` log collector had crashed, so no runtime SQL could be analyzed
4. **No decompilation possible** — ConfuserEx v1.0.0 protection prevents method body extraction
5. **No runtime debugging possible** — CHIFA is a production binary with no debug symbols or source code available

---

## Possible Root Cause Hypotheses

| Hypothesis | Likelihood | Basis |
|------------|------------|-------|
| H1: The compiled SQL includes a WHERE condition not met by experiment data (e.g., `facture.etat IN ('S','V')` or `qte > 0` or `signature IS NOT NULL`) | MEDIUM-HIGH | Multiple conditions could explain the filtering — any single condition would be sufficient |
| H2: `detail_bord` is populated by CHIFA's internal creation/validation workflow, not by a simple SQL query against existing data | MEDIUM | CHIFA may need to "process" invoices through its internal pipeline before they appear in the bordereau |
| H3: A session/state dependency — `detail_bord` depends on CHIFA's current user session, selected centre, or other application state | LOW-MEDIUM | Possible but speculative |
| H4: A combination of H1 + H2 — both the SQL conditions AND the workflow are required | MEDIUM | Most realistic scenario for a production application |

None of these hypotheses are confirmed. They must remain as hypotheses.

---

## Distinction Between Observed and Root Cause

```
Observed Cause (CONFIRMED):
  └── detail_bord DataTable was empty → bordereau invisible
        └── SQL INSERT does not populate detail_bord

Exact Root Cause (NOT DETERMINED):
  └── Why was detail_bord empty?
        ├── H1: WHERE clause filtered out the data
        ├── H2: Workflow step missing
        ├── H3: Session/state issue
        └── H4: Combination
```

---

## Key Takeaway

The experiment CONFIRMED that `detail_bord` is not a live SQL view.

The experiment did NOT determine the exact root cause of the filtering.

Any BM Pharma integration design must account for this uncertainty and assume that direct SQL INSERT alone cannot guarantee bordereau visibility in CHIFA's Visualiser Bordereau.
