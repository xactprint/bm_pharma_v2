# BM-PHASE-009 — Final Report

**Reverse Engineering of Visualiser Bordereau & detail_bord DataTable**

> **Date:** 2026-07-28
> **Status:** COMPLETE
> **Classification:** Confidential — BM Pharma Internal

---

## 1. Executive Summary

This report documents the complete investigation into the CHIFA-OFFICINE "Visualiser Bordereau" feature and its underlying `detail_bord` DataTable. The investigation combined static analysis (IL dumps, database schema, configuration files), dynamic observation (experiment TST003), and architectural reconstruction.

**Key finding:** `detail_bord` is a .NET in-memory Typed DataTable (not a PostgreSQL table), populated by compiled SQL embedded in the ConfuserEx-protected CHIFA binary. Direct SQL INSERT into PostgreSQL does not populate this DataTable, which explains why bordereaux created outside CHIFA are invisible in the "Visualiser Bordereau" interface.

**Status:** The architecture is understood at the structural level, but the exact population mechanism (SQL query, WHERE clause, business logic) cannot be fully determined without decompilation or runtime SQL capture.

---

## 2. Scope

| Dimension | Coverage |
|-----------|----------|
| **Target** | CHIFA-OFFICINE v3.0.4.3, "Visualiser Bordereau" form |
| **Component** | `DataSet1.detail_bordDataTable` population mechanism |
| **Analysis methods** | IL dump analysis, database schema analysis, config analysis, experiment TST003 |
| **Constraints** | Read-only — no INSERT/UPDATE/DELETE beyond controlled experiment (rolled back) |
| **Out of scope** | Other CHIFA DataSets (DataSet1_1, DataSet2, DataSet3, etc.), web services, FTP |

---

## 3. Environment

| Component | Detail |
|-----------|--------|
| Machine | Windows 10 (local workstation) |
| CHIFA version | 3.0.4.3 (.NET Framework 4.0 Client Profile) |
| Protection | ConfuserEx v1.0.0 (full assembly encryption + LZMA compression) |
| Database | PostgreSQL 9.3.4 (embedded, `CHIFA_OFFICINE` database) |
| Database schema | 48 tables, 0 triggers, 5 foreign keys, 2 sequences |
| Database auth | TRUST (no password), user `pharm` |
| Logging | NLog (ERROR level only), PostgreSQL `log_statement=all` (log collector crashed) |
| Sources | IL dumps (`full_dump.il`, `fbordereau.il`, `facture_ctrl.il`), CONFIG.xml, settings.cfg, postgresql.conf, pg_hba.conf |

---

## 4. Evidence

See BM-PHASE-009-EVIDENCE-MATRIX.md for the complete evidence inventory (30 items: 18 CONFIRMED, 4 PROBABLE, 1 RECONSTRUCTED, 7 NOT DETERMINED).

**Key evidence sources:**
- **IL dump** (`full_dump.il:52-148`): Confirms `detail_bordDataTable`, `detail_bordRow`, `detail_bordRowChangeEvent`, `detail_bordRowChangeEventHandler` as nested types of `DataSet1`
- **pg_dump / schema analysis**: Confirms no `detail_bord` table exists in PostgreSQL
- **Experiment TST003**: Confirms direct SQL INSERT populates `facture` (visible in Consultation Facture) but does not populate `detail_bord` (bordereau invisible in Visualiser Bordereau)
- **Process inspection**: Confirms PostgreSQL `--forkcol` log collector process crashed — no SQL capture

---

## 5. detail_bord Architecture

```
DataSet1 (CHIFA_OFFICINE.DataSet1)
  ├── detail_factDataTable          (PostgreSQL-backed: detail_fact)
  ├── detail_bordDataTable          (IN-MEMORY ONLY — no PostgreSQL table)
  ├── remarquesDataTable            (IN-MEMORY ONLY)
  ├── parametreDataTable            (PostgreSQL-backed: parametre)
  ├── param_demande_cmDataTable     (IN-MEMORY ONLY)
  └── parametre_journalDataTable    (IN-MEMORY ONLY)
```

- `detail_bord` is a Typed DataTable with full typed row infrastructure (`detail_bordRow`, change events, event handlers)
- It has **no corresponding PostgreSQL table** — it exists only at application runtime
- It is populated by **compiled SQL** embedded in the ConfuserEx-protected CHIFA binary
- It is consumed by `FormVisualiserBordereau` and RDLC reports `Report_Bord_1.rdlc` / `Report_Bord_2.rdlc`

---

## 6. Visualiser Bordereau Workflow

```
User → Menu → FormVisualiserBordereau
  → [Load method — NOT DETERMINED]
    → [SQL command — NOT DETERMINED]
      → Npgsql query against PostgreSQL
        → [DataAdapter.Fill — NOT DETERMINED]
          → DataSet1.detail_bord (in-memory)
            → DataGridView display
            → Report_Bord_1.rdlc / Report_Bord_2.rdlc
```

The load method, SQL command, and DataAdapter are all compiled into the encrypted CHIFA binary. Their exact implementation cannot be determined from available evidence.

---

## 7. SQL Analysis

**Reconstructed query** (PROBABLE — not confirmed):

```sql
SELECT b.num_bord, b.code_centre, b.etat, b.date_ouverture, b.date_cloture,
       f.num_fact, f.num_assure, ben.nom, ben.prenom,
       df.num_enr, m.nom_com, df.qte, df.ppa, df.mont, df.mont_as
FROM bordereau b
JOIN facture f ON f.num_bord = b.num_bord
LEFT JOIN beneficiaire ben ON f.num_assure = ben.num_assure AND f.rang_ad = ben.rang_ad
JOIN detail_fact df ON df.num_fact = f.num_fact
JOIN medicament m ON df.num_enr = m.num_enr
WHERE b.num_bord = @num_bord;
```

**Status of each element:**

| Element | Status |
|---------|--------|
| `bordereau` | CONFIRMED (must be source) |
| `facture` | CONFIRMED (must be source) |
| `detail_fact` | CONFIRMED (must be source) |
| `medicament` | PROBABLE (medication names required for display) |
| `beneficiaire` | PROBABLE (beneficiary names required for display) |
| JOIN conditions | RECONSTRUCTED (based on FK relationships) |
| WHERE clause | NOT DETERMINED (exact conditions unknown) |
| ORDER BY | NOT DETERMINED |
| Parameters | NOT DETERMINED |

---

## 8. TST003 Experiment

**Objective:** Validate that `detail_bord` is an in-memory DataTable, not a live SQL view.

**Setup:** INSERT into `facture` + `detail_fact` + `bordereau` with all FK constraints satisfied. Counter coordination (`next_num_fact`, `next_num_bord`) performed correctly.

**Result:**
- "Consultation Facture": **Visible** (confirms data is valid in PostgreSQL)
- "Visualiser Bordereau": **Invisible** (confirms DataTable was not populated)

**Rollback:** Complete (11/11 checks, baseline restored to facture=0, detail_fact=0, bordereau=0, signature=0)

---

## 9. Root Cause

| Level | Finding | Status |
|-------|---------|--------|
| Observable cause | `detail_bord` DataTable was empty → bordereau invisible | **CONFIRMED** |
| Mechanical cause | SQL INSERT does not execute CHIFA's compiled fill pipeline | **CONFIRMED** |
| Exact root cause | Unknown — the compiled SQL WHERE clause and/or CHIFA workflow conditions were not met | **NOT DETERMINED** |

The exact root cause (whether a WHERE condition, a missing workflow step, a session dependency, or a combination) cannot be determined without:
1. Decompilation of the CHIFA binary (requires ConfuserEx deobfuscation)
2. Runtime SQL capture (requires PostgreSQL log collector restart)
3. Runtime debugging (requires debug symbols)

---

## 10. Known Visibility Rules

**CONFIRMED:**
- `detail_bord` must be populated by CHIFA-OFFICINE at runtime
- Direct SQL INSERT alone is insufficient
- "Consultation Facture" is not affected — it reads `facture` table directly

**NOT DETERMINED:**
- Whether `facture.etat` must be a specific value (S, V, or B)
- Whether `facture.signature` must be non-null
- Whether `detail_fact.qte > 0` is a condition
- Whether additional workflow steps are required (e.g., opening each invoice in CHIFA)
- Whether session/state dependencies exist

---

## 11. Unknowns

| # | Unknown | Impact | Resolution Path |
|---|---------|--------|-----------------|
| U01 | Exact SQL WHERE clause | Blocks full replication of detail_bord | PostgreSQL log collector restart OR ConfuserEx deobfuscation |
| U02 | CHIFA workflow pre-processing steps | May block bordereau visibility | Runtime observation with controlled test in CHIFA |
| U03 | Additional business filters (état, signature, dates) | May exclude valid data | SQL logging + CHIFA workflow testing |
| U04 | ORDER BY clause | Low impact — sort order only | SQL logging |
| U05 | Parameter naming and types | Low impact — standard Npgsql parameters | SQL logging |
| U06 | Error handling when detail_bord is empty | Low impact — form simply shows no data | UI observation |
| U07 | Whether `cloturerbord()` accepts externally-created bordereaux | MEDIUM impact on architecture decision | Direct test in test environment |

---

## 12. Security

- No security vulnerabilities were introduced by this analysis
- All analysis was read-only (except the controlled, rolled-back TST003 experiment)
- No credentials, private keys, or sensitive data were extracted
- CHIFA-OFFICINE remains fully operational after analysis
- The ConfuserEx protection effectively prevents static analysis of business logic — this is a design feature, not a vulnerability

---

## 13. Architectural Impact

| Area | Impact | Severity |
|------|--------|----------|
| BM Pharma invoice creation | None — works via direct SQL | LOW |
| BM Pharma bordereau creation | **Visibility gap** — bordereaux invisible in CHIFA UI | MEDIUM |
| BDLC report printing | **Blocked** — reports depend on `detail_bord` DataTable | MEDIUM |
| Signature pipeline | None — CHIFA handles signing independently | LOW |
| Clôture pipeline | **Unknown** — may or may not accept externally-created bordereaux | MEDIUM |
| Integration architecture | **Provisional** — Option D recommended until unknowns are resolved | MEDIUM |

---

## 14. Recommendation

**Provisional recommendation: Option D (Hybrid)**

BM Pharma creates `facture` + `detail_fact` directly in PostgreSQL. The pharmacist then opens each invoice in CHIFA's native "Vente Chifa" interface — this ensures CHIFA's full validation and DataTable population pipeline runs. CHIFA handles bordereau creation, signing, clôture, and transmission.

This approach:
- Maximizes compatibility with CHIFA's unknown business logic
- Minimizes risk of incorrect replication of undocumented rules
- Preserves CHIFA's audit trail and validation
- Provides a safe path that can be revised once the `detail_bord` pipeline is fully understood

**Final architectural decision requires resolution of unknowns U01–U07** before moving to Option A (full BM Pharma control) or Option C (replication).

---

## 15. Limitations

1. **ConfuserEx protection prevents static analysis** of method bodies — all business logic in the main EXE is opaque
2. **PostgreSQL logging was non-functional** during the investigation period (log collector crashed)
3. **No .NET decompilation possible** for the main assembly — ildasm reports "not a PE file"
4. **No runtime profiling** — CHIFA's NLog configuration captures ERROR-level events only
5. **Reconstructed SQL is unverified** — may differ from actual compiled query
6. **Single experiment (TST003)** — limited scope; additional experiments could provide more insight
7. **No access to CHIFA source code or debug symbols** — all analysis is black-box

---

## 16. Next Steps

| Step | Action | Priority | Dependencies |
|------|--------|----------|--------------|
| 1 | Restart PostgreSQL logging | HIGH | Access to CHIFA machine, PostgreSQL service restart |
| 2 | Run SQL capture during CHIFA Visualiser Bordereau operation | HIGH | Step 1 complete |
| 3 | Test CHIFA workflow: open externally-created facture in Vente Chifa | HIGH | Available CHIFA session |
| 4 | Test `cloturerbord()` with externally-created bordereau | MEDIUM | Test environment (non-production) |
| 5 | Investigate ConfuserEx deobfuscation tools | MEDIUM | Research approval |
| 6 | Decide on final architecture based on steps 1-5 | HIGH | All steps above |

---

*End of BM-PHASE-009 Final Report*

*Documents in this phase:*
1. BM-PHASE-009-EXECUTIVE-REPORT.md
2. BM-PHASE-009-VISUALISER-BORDEREAU-CALLGRAPH.md
3. BM-PHASE-009-DETAIL-BORD-ANALYSIS.md
4. BM-PHASE-009-SQL-ANALYSIS.md
5. BM-PHASE-009-VISIBILITY-RULES.md
6. BM-PHASE-009-TST003-ROOT-CAUSE.md
7. BM-PHASE-009-CHIFA-WORKFLOW.md
8. BM-PHASE-009-ARCHITECTURE-RECOMMENDATION.md
9. BM-PHASE-009-EVIDENCE-MATRIX.md
10. BM-PHASE-009-FINAL-REPORT.md
