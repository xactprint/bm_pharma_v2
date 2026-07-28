# BM-PHASE-009 — detail_bord Analysis

**In-Memory DataTable Architecture**

> **Date:** 2026-07-28
> **Status:** COMPLETE
> **Classification:** Confidential — BM Pharma Internal

---

## 1. Identity

```
Full Name:   CHIFA_OFFICINE.DataSet1.detail_bordDataTable
Assembly:    CHIFA_OFFICINE (.NET Framework 4.0 Client Profile)
Protection:  ConfuserEx v1.0.0 (LZMA-compressed, full assembly encryption)
Type:        Typed DataTable (System.Data.DataTable subclass)
Nested in:   CHIFA_OFFICINE.DataSet1
```

---

## 2. IL Evidence

The IL dump of `full_dump.il` (lines 52-148) confirms the following nested types within `DataSet1`:

```
.class extern public CHIFA_OFFICINE.DataSet1
  .class extern nested public detail_factDataTable
  .class extern nested public detail_bordDataTable          ← detail_bord
  .class extern nested public remarquesDataTable
  .class extern nested public parametreDataTable
  .class extern nested public param_demande_cmDataTable
  .class extern nested public parametre_journalDataTable

  .class extern nested public detail_factRow
  .class extern nested public detail_bordRow                ← Typed row
  .class extern nested public remarquesRow
  .class extern nested public parametreRow

  .class extern nested public detail_factRowChangeEvent
  .class extern nested public detail_bordRowChangeEvent     ← Change event
  .class extern nested public remarquesRowChangeEvent

  .class extern nested public detail_factRowChangeEventHandler
  .class extern nested public detail_bordRowChangeEventHandler  ← Event handler
  .class extern nested public remarquesRowChangeEventHandler
```

All types are declared as `class extern` — their method bodies are in the encrypted assembly and are not extractable.

---

## 3. PostgreSQL Table Mapping

| Aspect | Value |
|--------|-------|
| Corresponding PostgreSQL table? | **NO** |
| Type | In-memory only (System.Data.DataTable) |
| Population mechanism | Compiled SQL in CHIFA binary |
| Persistence | Not persisted — rebuilt on each form load |
| Lifecycle | Created when `FormVisualiserBordereau` opens, destroyed on close |

`detail_bord ≠ table PostgreSQL`

`detail_bord = DataTable .NET en mémoire`

---

## 4. Consumed By

| Consumer | Type | Purpose | Confidence |
|----------|------|---------|------------|
| `FormVisualiserBordereau` | WinForms Form | Display bordereau line items in UI | CONFIRMED |
| `Report_Bord_1.rdlc` | RDLC Report | Bordereau report page 1 | CONFIRMED |
| `Report_Bord_2.rdlc` | RDLC Report | Bordereau report page 2 | CONFIRMED |

Both RDLC reports bind directly to `DataSet1.detail_bord` as their data source.

---

## 5. Inferred Columns

Based on the reconstructed SQL query, the `detail_bord` DataTable likely contains these columns:

| # | Column | Probable .NET Type | Source Table | Notes |
|---|--------|--------------------|--------------|-------|
| 1 | `num_bord` | String | `bordereau.num_bord` | Bordereau number |
| 2 | `code_centre` | String | `bordereau.code_centre` | Centre code |
| 3 | `etat` | String | `bordereau.etat` | Bordereau state |
| 4 | `date_ouverture` | DateTime | `bordereau.date_ouverture` | Opening date |
| 5 | `date_cloture` | DateTime | `bordereau.date_cloture` | Closure date |
| 6 | `num_fact` | String | `facture.num_fact` | Invoice number |
| 7 | `num_assure` | String | `facture.num_assure` | Insurance number |
| 8 | `nom` | String | `beneficiaire.nom` | Beneficiary surname |
| 9 | `prenom` | String | `beneficiaire.prenom` | Beneficiary first name |
| 10 | `num_enr` | String | `detail_fact.num_enr` | Medication registration code |
| 11 | `nom_com` | String | `medicament.nom_com` | Medication commercial name |
| 12 | `qte` | Decimal | `detail_fact.qte` | Quantity dispensed |
| 13 | `ppa` | Decimal | `detail_fact.ppa` | PPA price |
| 14 | `mont` | Decimal | `detail_fact.mont` | Total amount (qte × ppa) |
| 15 | `mont_as` | Decimal | `detail_fact.mont_as` | Assurance amount |

**Important:** This column list is **reconstructed** from the probable SQL JOIN (see BM-PHASE-009-SQL-ANALYSIS.md). The exact column set in the actual DataTable may differ — it is determined by the compiled SQL query inside the protected binary.

---

## 6. Relationship to DataSet1_1

`DataSet1_1` (`CHIFA_OFFICINE.DataSet1_1`) is an inferred subset of `DataSet1` that also contains `detail_bord` with identical column definitions. It is not visible in the IL dump — its existence is inferred from form usage patterns and the naming convention.

---

## 7. Key Architectural Insight

`detail_bord` is **not a PostgreSQL view, not a materialized view, not a SQL function, and not a trigger-fed table**. It is a pure application-level construct:

```
PostgreSQL Tables                  CHIFA-OFFICINE Runtime
─────────────────                  ──────────────────────
bordereau         ──┐
facture           ──┤  Compiled SQL  ──►  DataSet1.detail_bord  ──►  FormVisualiserBordereau
detail_fact       ──┤                  ──►  Report_Bord_1.rdlc
medicament        ──┤                  ──►  Report_Bord_2.rdlc
beneficiaire      ──┘
```

This means any data written directly to PostgreSQL is **invisible** to `detail_bord` until CHIFA-OFFICINE itself executes its compiled SQL and populates the DataTable.
