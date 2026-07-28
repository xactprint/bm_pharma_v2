# BM-PHASE-009 — Visibility Rules

**Rules Governing Bordereau Visibility in Visualiser Bordereau**

> **Date:** 2026-07-28
> **Status:** PARTIAL — full rule set not determined
> **Classification:** Confidential — BM Pharma Internal

---

## Rules CONFIRMED

| # | Rule | Evidence | Source |
|---|------|----------|--------|
| R01 | `detail_bord` is an in-memory .NET DataTable, not a PostgreSQL table | IL dump shows `detail_bordDataTable` nested in `DataSet1`; no `detail_bord` table in pg_dump | IL analysis / Phase 4 |
| R02 | `detail_bord` must be populated at runtime by CHIFA-OFFICINE to display bordereau data | TST003: bordereau invisible despite valid SQL INSERT | Experiment |
| R03 | Direct SQL INSERT into `facture` + `detail_fact` + `bordereau` does NOT populate `detail_bord` | TST003: all PostgreSQL constraints satisfied, bordereau still invisible | Experiment |
| R04 | A facture created via direct SQL IS visible in "Consultation Facture" | TST003: `SELECT * FROM facture` shows the record | Experiment |
| R05 | `FormVisualiserBordereau` consumes `DataSet1.detail_bord` as its data source | UI architecture analysis | Phase 6 |
| R06 | `Report_Bord_1.rdlc` and `Report_Bord_2.rdlc` bind to `DataSet1.detail_bord` | RDLC report analysis | Phase 6 |
| R07 | `detail_bord` has typed row infrastructure (`detail_bordRow`, change events, event handlers) | IL dump (lines 110, 116, 98) | IL analysis |
| R08 | CHIFA-OFFICINE binary is ConfuserEx v1.0.0 protected — method bodies not extractable | IL dump shows only extern type references, no method IL | IL analysis |

---

## Rules PROBABLE

| # | Rule | Logic | Confidence |
|---|------|-------|------------|
| R09 | `detail_bord` is populated by a SQL query joining `bordereau` → `facture` → `detail_fact` → `medicament` → `beneficiaire` | Logical requirement for displaying bordereau details; partially reconstructed | MEDIUM |
| R10 | The bordereau number (`b.num_bord`) is used as a filter parameter | Visualiser Bordereau displays one bordereau at a time | MEDIUM |
| R11 | The SQL query is embedded in the DataSet typed schema (`.xsd` file or code-behind) | Standard .NET DataSet pattern with TableAdapter | MEDIUM |
| R12 | The DataTable is cleared and refilled each time the form opens | Standard WinForms DataSet lifecycle | MEDIUM |

---

## Rules NOT DETERMINED

| # | Rule | Notes |
|---|------|-------|
| R13 | WHERE clause conditions beyond bordereau number | Could include `etat`, `qte > 0`, `signature IS NOT NULL`, date filters, centre filters — none confirmed |
| R14 | Whether `etat` is required | Possibly filters by open (NULL/'O') or closed ('C') bordereaux |
| R15 | Whether `facture.signature` must be non-null | May require signed invoices for display |
| R16 | Whether `detail_fact.qte > 0` is a condition | Zero-quantity lines may be excluded |
| R17 | Whether ORDER BY is applied | Sort order for display/report is unknown |
| R18 | Whether additional business logic runs before DataTable population | E.g., validation checks, CM status checks, signature verification |
| R19 | Whether the DataTable population depends on CHIFA user session state | May require logged-in user, selected centre, open period |
| R20 | Whether data is loaded from PostgreSQL only or also from application cache | Cache layer possible but unconfirmed |

---

## Visibility Decision Tree

```
POSTGRESQL DATA
    │
    ├── Consultation Facture
    │     └── Direct SELECT from facture table
    │           └── Data visible ─── CONFIRMED
    │
    └── Visualiser Bordereau
          └── CHIFA-OFFICINE compiled SQL
                └── DataSet1.detail_bord population
                      ├── If populated → visible
                      └── If not populated → invisible
                            └── TST003 confirmed this path
```

---

## Summary

| Visibility Route | Direct SQL INSERT works? | Confidence |
|------------------|-------------------------|------------|
| Consultation Facture | YES | CONFIRMED (TST003) |
| Visualiser Bordereau | NO | CONFIRMED (TST003) |
| RDLC Report_Bord_1 | NO (same DataTable) | CONFIRMED |
| RDLC Report_Bord_2 | NO (same DataTable) | CONFIRMED |

The critical gap: the exact conditions under which CHIFA-OFFICINE populates `detail_bord` remain unknown. Without this knowledge, BM Pharma cannot guarantee that a bordereau created via direct SQL will be visible in the CHIFA interface.
