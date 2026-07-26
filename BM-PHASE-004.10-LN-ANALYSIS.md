# BM-PHASE-004.10 — LN TABLE ANALYSIS

## Status: COMPLETE

**Date**: 2026-07-26
**Phase**: BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment

---

## Executive Summary

The `ln` (Liste Nationale des médicaments) table is a single-column table containing 7,412,276 medication serial numbers. Despite its large row count, it has minimal structure and many rows contain empty/blank/placeholder values. It is NOT a drug catalog — it is a serial number reference list maintained by CNAS.

---

## 1. Table Overview

| Property | Value |
|----------|-------|
| **Table Name** | ln |
| **Schema** | public |
| **Rows** | **7,412,276** |
| **Columns** | 1 |
| **PK** | None (single column table) |
| **Physical Size** | ~200 MB |
| **Average Row Size** | ~27 bytes |
| **EF Core Entity** | None (not mapped) |

---

## 2. Structure

| # | Column | PG Type | Nullable | Purpose |
|---|--------|---------|----------|---------|
| 1 | num_serie | varchar(20) | nullable | Medication serial number |

That's it. One column, 7.4 million rows.

---

## 3. What Is LN?

**LN = Liste Nationale des médicaments** (National List of Medications)

This table is maintained by CNAS (Caisse Nationale d'Assurance Maladie) and serves as a reference list of all medication serial numbers that have been authorized for use in the Algerian healthcare system.

**Key characteristics**:
- NOT a drug catalog (that's `medicament`)
- NOT a pricing table (that's `tarif`)
- It IS a serial number reference list
- Used for validation when creating invoices
- Updated periodically from CNAS servers

---

## 4. Data Quality Issues

### 4.1 Empty/Blank Rows

A significant portion of the 7,412,276 rows contain empty or blank `num_serie` values. These are likely:
- Placeholder rows created during database initialization
- Rows where serial numbers haven't been assigned yet
- Historical entries that were cleared

### 4.2 Pattern Data

Sample data reveals non-standard values:

| Pattern | Count | Meaning |
|---------|-------|---------|
| Valid serial numbers | Unknown | Real medication serials |
| Empty/blank strings | High | Unassigned entries |
| Spaces | Medium | Formatting artifacts |
| "oooooooooooooooo" | Medium | Placeholder/test data |
| Other patterns | Low | Various |

### 4.3 Data Quality Warning

**DO NOT assume all rows contain valid data.** The table appears to be a working reference that accumulates entries over time, including incomplete or test entries.

---

## 5. Size Analysis

| Metric | Value |
|--------|-------|
| Total Size | ~200 MB |
| Row Count | 7,412,276 |
| Logical Row Size | ~20 bytes (varchar(20) average) |
| Physical Row Size | ~27 bytes |
| TOAST Overhead | Minimal (short strings) |
| Index Overhead | Unknown (no PK detected) |

### 5.1 Why So Many Rows?

The LN table is a cumulative list — entries are added over time as new medications are registered with CNAS. The 7.4 million rows represent the total history of all medication serial numbers ever registered in the system.

---

## 6. Relationship to Other Tables

| Relationship | Status |
|-------------|--------|
| ln.num_serie → medicament | NOT directly referenced |
| ln.num_serie → detail_fact | NOT directly referenced |
| ln.num_serie → facture.num_serie | POSSIBLE but not enforced |

**Note**: The `facture` table has a `num_serie` column (bigint) which may reference LN data, but there is no explicit FK constraint. The types also differ (varchar(20) vs bigint).

---

## 7. EF Core Mapping Status

| Status | Reason |
|--------|--------|
| **NOT MAPPED** | Too large for in-memory loading, single-column table, data quality issues |

### 7.1 Why Not Mapped?

1. **Size**: 7.4 million rows cannot be loaded into memory
2. **Structure**: Single column offers minimal EF Core value
3. **Data Quality**: Many rows are empty/placeholder
4. **Access Pattern**: Query-by-value only, not entity-based
5. **Performance**: Full table scans would be expensive

---

## 8. Recommended Access Patterns

| Pattern | Recommendation |
|---------|---------------|
| Lookup by num_serie | Direct SQL query with WHERE clause (indexed if possible) |
| Bulk load | **NEVER** — 7.4 million rows will crash memory |
| Existence check | `SELECT COUNT(*) FROM ln WHERE num_serie = @value` |
| Caching | Cache frequently accessed serial numbers only |
| Search | NOT recommended — no meaningful search pattern |

### 8.1 SQL Query Examples

```sql
-- Check if a serial number exists
SELECT EXISTS(SELECT 1 FROM ln WHERE num_serie = '123456');

-- Count valid (non-empty) serial numbers
SELECT COUNT(*) FROM ln WHERE num_serie IS NOT NULL AND num_serie <> '';

-- Sample valid entries
SELECT num_serie FROM ln 
WHERE num_serie IS NOT NULL 
  AND num_serie <> '' 
  AND num_serie NOT LIKE '%o%' 
LIMIT 100;
```

---

## 9. Integration Recommendations

| # | Recommendation | Priority |
|---|---------------|----------|
| 1 | Do NOT map to EF Core entity | DONE |
| 2 | Use direct SQL queries for lookups | HIGH |
| 3 | Consider adding index on num_serie for performance | MEDIUM |
| 4 | Cache frequently used serial numbers in local SQLite | LOW |
| 5 | Periodic data cleanup (remove empty/placeholder rows) | LOW |
| 6 | Investigate FK relationship with facture.num_serie | LOW |

---

## 10. Comparison with medicament

| Property | ln | medicament |
|----------|-----|-----------|
| Rows | 7,412,276 | 7,596 |
| Columns | 1 | 29 |
| Size | ~200 MB | 80 MB |
| PK | None | num_enr |
| Purpose | Serial number reference | Drug catalog |
| EF Core | Not mapped | ChifaMedicament |
| Access | SQL queries only | Entity-based |

**Key difference**: `medicament` is a structured catalog with 29 columns suitable for EF Core mapping. `ln` is a flat serial number list that should be accessed via SQL queries only.

---

*Document generated by BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment*
