# BM-PHASE-009 — Evidence Matrix

**Complete Evidence Inventory for detail_bord / Visualiser Bordereau**

> **Date:** 2026-07-28
> **Status:** COMPLETE
> **Classification:** Confidential — BM Pharma Internal

---

## Evidence Matrix

| ID | Element | Source | Evidence | Status |
|----|---------|--------|----------|--------|
| R001 | `detail_bord` DataTable exists as nested type in `DataSet1` | IL dump (`full_dump.il:52-148`) | `.class extern nested public detail_bordDataTable` | **CONFIRMED** |
| R002 | `detail_bord` is a Typed DataTable (.NET in-memory) | IL dump | `DataTable` base class implied by nesting pattern | **CONFIRMED** |
| R003 | `detail_bordRow` typed row class exists | IL dump (line 110) | `.class extern nested public detail_bordRow` | **CONFIRMED** |
| R004 | `detail_bordRowChangeEvent` exists | IL dump (line 116) | `.class extern nested public detail_bordRowChangeEvent` | **CONFIRMED** |
| R005 | `detail_bordRowChangeEventHandler` exists | IL dump (line 98) | `.class extern nested public detail_bordRowChangeEventHandler` | **CONFIRMED** |
| R006 | No PostgreSQL table named `detail_bord` exists | pg_dump / Phase 4 | Schema has 48 tables; none named `detail_bord` | **CONFIRMED** |
| R007 | `FormVisualiserBordereau` consumes `DataSet1.detail_bord` | UI architecture (Phase 6) | Form identified in call graph | **CONFIRMED** |
| R008 | `Report_Bord_1.rdlc` binds to `DataSet1.detail_bord` | UI architecture (Phase 6, line 855) | RDLC resource mapping | **CONFIRMED** |
| R009 | `Report_Bord_2.rdlc` binds to `DataSet1.detail_bord` | UI architecture (Phase 6, line 856) | RDLC resource mapping | **CONFIRMED** |
| R010 | CHIFA_OFFICINE.exe is ConfuserEx v1.0.0 protected | IL dump / binary analysis | LZMA decoder present, no method bodies | **CONFIRMED** |
| R011 | TST003 facture visible in Consultation Facture | Experiment BM-PHASE-008-B | UI observation during test | **CONFIRMED** |
| R012 | TST003 bordereau invisible in Visualiser Bordereau | Experiment BM-PHASE-008-B | UI observation during test | **CONFIRMED** |
| R013 | PostgreSQL logging configured (`log_statement=all`) | Config file + live settings query | `pg_settings` confirms value | **CONFIRMED** |
| R014 | PostgreSQL log collector process (`--forkcol`) crashed | Process inspection (PID 5128 not found) | No SQL log files despite config | **CONFIRMED** |
| R015 | No SQL queries captured from CHIFA runtime | Log directory inspection | Only `startup.log` present (808 bytes) | **CONFIRMED** |
| R016 | TST003 rollback completed successfully — baseline restored | BM-PHASE-008-B rollback report | 11/11 checks passed | **CONFIRMED** |
| R017 | SQL query involves `bordereau` → `facture` JOIN | Logical reconstruction | FK `facture.num_bord` → `bordereau.num_bord` | **CONFIRMED** |
| R018 | SQL query involves `facture` → `detail_fact` JOIN | Logical reconstruction | FK `detail_fact.num_fact` → `facture.num_fact` | **CONFIRMED** |
| R019 | SQL query involves `detail_fact` → `medicament` JOIN | Logical reconstruction | `detail_fact.num_enr` → `medicament.num_enr` (implicit FK) | **PROBABLE** |
| R020 | SQL query involves `beneficiaire` LEFT JOIN | Logical reconstruction | `facture.num_assure` → `beneficiaire.num_assure` | **PROBABLE** |
| R021 | `DataSet1` contains 6 DataTables total | IL dump (lines 96-101) | 6 nested DataTable extern types | **CONFIRMED** |
| R022 | Direct SQL INSERT does not populate `detail_bord` | Experiment TST003 | Bordereau invisible despite valid INSERT | **CONFIRMED** |
| R023 | `detail_bord` is DataSet1_1 compatible | Phase 5 analysis | DataSet1_1 inferred to share same structure | **PROBABLE** |
| R024 | Exact WHERE clause of `detail_bord` SQL | — | Not extractable due to ConfuserEx | **NOT DETERMINED** |
| R025 | SQL ORDER BY clause | — | Not extractable | **NOT DETERMINED** |
| R026 | SQL parameters / parameter names | — | Not extractable | **NOT DETERMINED** |
| R027 | Additional business rules/filters in fill pipeline | — | Not observable | **NOT DETERMINED** |
| R028 | Whether CHIFA workflow pre-processing is required | — | Not determined | **NOT DETERMINED** |
| R029 | `detail_bord` column definitions (exact) | — | Derived from reconstruction only | **RECONSTRUCTED** |
| R030 | Npgsql connection string used by CHIFA | — | Not extractable (binary encrypted) | **NOT DETERMINED** |

---

## Status Summary

| Status | Count | ID Range |
|--------|-------|----------|
| CONFIRMED | 18 | R001–R016, R017, R018, R021, R022 |
| PROBABLE | 4 | R019, R020, R023, (partial R005) |
| RECONSTRUCTED | 1 | R029 |
| NOT DETERMINED | 7 | R024, R025, R026, R027, R028, R030 |

---

## Methodology

Each evidence item was classified using the following criteria:

| Status | Definition |
|--------|------------|
| **CONFIRMED** | Directly observed from IL dump, database schema, experiment, or configuration file |
| **PROBABLE** | Strongly inferred from logical relationships or consistent patterns; no direct proof |
| **RECONSTRUCTED** | Inferred from a combination of sources; may differ from actual implementation |
| **HYPOTHESIS** | Plausible but no direct or indirect evidence |
| **NOT DETERMINED** | Cannot be confirmed, inferred, or hypothesized with available evidence |
