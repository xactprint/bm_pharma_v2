# BM-PHASE-004.12-CRITICAL-TABLES.md
# Phases 3-8: Critical Table Deep-Dive

**Date:** 2026-07-27
**Status:** ✅ ALL TABLES VALIDATED

---

## facture

| Property | Value |
|----------|-------|
| Columns | 53 |
| Primary Key | num_fact |
| Foreign Key | num_bord → bordereau(num_bord) |
| Row Count | 0 |
| Purpose | Main invoice header table |

Key columns include: num_fact, num_bord, code_ps, date_fact, mont_total, mont_paye, mont_sub, and numerous billing/detail fields. The table is the central fact in the billing domain.

## detail_fact

| Property | Value |
|----------|-------|
| Columns | 20 |
| Primary Key | (num_fact, num_enr, ppa) |
| Foreign Key | num_fact → facture(num_fact) |
| Row Count | 0 |
| Purpose | Invoice line items |

Each row represents one medication line on an invoice. Composite PK ensures uniqueness per invoice+line+beneficiary.

## bordereau

| Property | Value |
|----------|-------|
| Columns | 11 |
| Primary Key | num_bord |
| Unique | num_bord |
| Auto-increment | id_bord (bigint, via bordereau_id_bord_seq) |
| Row Count | 0 |
| Purpose | Batch/submission header for grouped invoices |

## parametre

| Property | Value |
|----------|-------|
| Columns | 58 |
| Primary Key | code_ps |
| Row Count | 1 (singleton) |
| Purpose | Application-wide configuration |

### Critical parametre values:

| Column | Value | Meaning |
|--------|-------|---------|
| code_ps | 1234567890 | Pharmacy identifier |
| nom | PHARMACIE | Pharmacy name |
| prenom | EXEMPLE | Example placeholder |
| code_centre | 11600 | Centre code |
| next_num_fact | 1 | Next invoice number (not yet used) |
| next_num_bord | 215 | Next bordereau number |
| version | 3.0.4.3 | Software version |
| annee | 2024 | Fiscal year |
| mont_maj_fae | 5 | FAE surcharge amount |
| mont_maj_sub | 15 | Subsidy surcharge amount |
| taux_maj_local | 20 | Local rate percentage |
| access_token | NULL | OAuth token (empty) |
| refresh_token | NULL | OAuth refresh (empty) |

**Structure:** Flat columns, NOT key/value. Column count is 58 (not 57 as previously estimated).

## medicament

| Property | Value |
|----------|-------|
| Columns | 29 |
| Primary Key | num_enr |
| Row Count | 7,596 |
| Purpose | Medication catalog |

### Critical column name: `nom_com` (NOT `designation`)

Sample data:
| num_enr | nom_com | tarif_ref |
|---------|---------|-----------|
| 00010 | ZYRTEC | 60.00 |
| 00011 | VIRLIX | 90.00 |

## signature

| Property | Value |
|----------|-------|
| Columns | 2 |
| Primary Key | num_fact |
| Row Count | 0 |
| Purpose | Digital signature per invoice |

Columns: `num_fact` (int) and `sign` (text). Simple 1:1 with facture.

## ln (Liste Noire / Black List)

| Property | Value |
|----------|-------|
| Columns | 1 |
| Primary Key | None (single column table) |
| Column Type | num_serie varchar(**16**) — NOT varchar(20) |
| Row Count | 7,412,276 |
| Distinct Values | 3,706,134 |
| Purpose | Blacklisted serial numbers |

**Critical finding:** Column is `varchar(16)`, not `varchar(20)`. This affects EF Core mapping and validation rules. The high row count (7.4M) with ~50% uniqueness indicates significant serial number reuse/duplication in the blacklist.

## Table Relationship Map

```
bordereau (0 rows)
  └── facture (0 rows) [FK: num_bord]
        ├── detail_fact (0 rows) [FK: num_fact]
        ├── signature (0 rows) [FK: num_fact]
        └── facture_cm → detail_fact_cm (0 rows)

parametre (1 row) [singleton, no FK]
medicament (7,596 rows) [no FK, standalone catalog]
ln (7,412,276 rows) [no FK, standalone blacklist]
```
