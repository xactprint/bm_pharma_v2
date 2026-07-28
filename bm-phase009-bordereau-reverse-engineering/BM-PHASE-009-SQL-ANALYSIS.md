# BM-PHASE-009 — SQL Analysis

**Reconstructed Query for detail_bord DataTable Population**

> **Date:** 2026-07-28
> **Status:** PARTIAL — exact SQL not determined
> **Classification:** Confidential — BM Pharma Internal

---

## Reconstructed SQL

Based on analysis of `DataSet1`, `FormVisualiserBordereau`, and the PostgreSQL schema, the following SQL query is **reconstructed** as the probable query used to populate `detail_bord`:

```sql
SELECT
    b.num_bord,
    b.code_centre,
    b.etat,
    b.date_ouverture,
    b.date_cloture,
    f.num_fact,
    f.num_assure,
    ben.nom,
    ben.prenom,
    df.num_enr,
    m.nom_com,
    df.qte,
    df.ppa,
    df.mont,
    df.mont_as
FROM bordereau b
JOIN facture f ON f.num_bord = b.num_bord
LEFT JOIN beneficiaire ben
    ON f.num_assure = ben.num_assure
    AND f.rang_ad = ben.rang_ad
JOIN detail_fact df ON df.num_fact = f.num_fact
JOIN medicament m ON df.num_enr = m.num_enr
WHERE b.num_bord = @num_bord
ORDER BY f.num_fact, df.num_enr;
```

**Note:** The exact column list, JOIN types (LEFT vs INNER), WHERE conditions, ORDER BY clause, and parameter names are **reconstructed hypotheses**. They may differ from the actual compiled SQL.

---

## Element Status Matrix

| Element | Status | Confidence | Evidence |
|---------|--------|------------|----------|
| `bordereau` table | CONFIRMED | HIGH | Must be source for bordereau display |
| `facture` table | CONFIRMED | HIGH | Contains invoices linked to bordereau via `num_bord` |
| `detail_fact` table | CONFIRMED | HIGH | Contains line items linked to facture via `num_fact` |
| `medicament` table | PROBABLE | MEDIUM | Provides medication names; likely joined on `num_enr` |
| `beneficiaire` table | PROBABLE | MEDIUM | Provides beneficiary names; LEFT JOIN likely |
| INNER JOIN `facture`–`bordereau` | PROBABLE | MEDIUM | Logical requirement for bordereau display |
| INNER JOIN `detail_fact`–`facture` | PROBABLE | MEDIUM | Detail lines require parent invoice |
| LEFT JOIN `beneficiaire`–`facture` | HYPOTHESIS | LOW | Beneficiary data may not always exist |
| INNER JOIN `medicament`–`detail_fact` | PROBABLE | MEDIUM | Medication code lookup |
| WHERE `b.num_bord = @param` | PROBABLE | MEDIUM | Filtering by selected bordereau |
| WHERE additional conditions | NOT DETERMINED | N/A | Any filters on etat, qte, signature, etc. |
| ORDER BY clause | NOT DETERMINED | N/A | Sort order unknown |
| Column list | RECONSTRUCTED | LOW-MEDIUM | Based on RDLC report data fields |
| Parameter name | NOT DETERMINED | N/A | `@num_bord` is a convention — unknown actual name |
| SQL dialect | Npgsql-compatible | HIGH | CHIFA uses Npgsql for PostgreSQL access |

---

## Where Clause Unknown

The following are potentially in the WHERE clause but are **NOT determined**:

| Possible Condition | Reason for Uncertainty |
|--------------------|-----------------------|
| `b.etat IS NOT NULL` or `b.etat = 'O'` | CHIFA may filter by open bordereaux only |
| `df.qte > 0` | Filter out zero-quantity lines |
| `f.etat IN ('S', 'V', 'C')` | May only show signed/validated invoices |
| `f.signature IS NOT NULL` | May require signed invoices only |
| Date range filter | May limit by date_ouverture or date_soin |
| Centre filter | May restrict to user's centre |

None of these conditions are confirmed. They must remain in the NOT DETERMINED category.

---

## Tables Referenced (Schema Context)

| Table | Columns (relevant) | PK | FK |
|-------|-------------------|----|----|
| `bordereau` | `num_bord`, `code_centre`, `etat`, `date_ouverture`, `date_cloture` | `num_bord` | — |
| `facture` | `num_fact`, `num_assure`, `rang_ad`, `num_bord`, `mont_off`, `mont_as`, `mont_fact` | `num_fact` | `num_bord` → `bordereau` |
| `detail_fact` | `num_fact`, `num_enr`, `qte`, `ppa`, `mont`, `mont_as` | `(num_fact, num_enr, ppa)` | `num_fact` → `facture` |
| `medicament` | `num_enr`, `nom_com`, `dosage` | `num_enr` | — |
| `beneficiaire` | `num_assure`, `rang_ad`, `nom`, `prenom` | `(num_assure, rang_ad)` | — |

---

## Why the SQL Cannot Be Extracted

1. **ConfuserEx v1.0.0** encrypts the entire assembly — method bodies are in LZMA-compressed encrypted resources
2. **ILDASM** cannot decompile — reports "File not found or not a PE file" for the main assembly
3. **No debug symbols** — CHIFA is a production deployment, not a development build
4. **PostgreSQL `--forkcol` crashed** — the log collector process died before any SQL was captured
5. **No runtime profiling** — CHIFA's NLog configuration only logs ERROR level messages, not SQL

---

## Conclusion

The SQL query that populates `detail_bord` can only be fully and definitively determined through:

1. Decompilation of the CHIFA_OFFICINE.exe assembly (requires ConfuserEx deobfuscation)
2. Runtime SQL capture via a working PostgreSQL logging configuration (requires log collector restart)
3. Network-level SQL capture (requires MITM proxy between CHIFA and PostgreSQL)

Until one of these methods succeeds, the reconstructed SQL above represents the best available approximation.
