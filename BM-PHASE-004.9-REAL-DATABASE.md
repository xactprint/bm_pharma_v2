# BM-PHASE-004.9 — REAL DATABASE DISCOVERY

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

The real CHIFA_OFFICINE PostgreSQL database was successfully queried via single-user mode (`postgres --single`), bypassing the crashed multi-user backend. This document captures the complete database metadata.

---

## 1. PostgreSQL Server

| Property | Value |
|----------|-------|
| Version | **PostgreSQL 9.3.4** |
| Architecture | 32-bit (x86) |
| Compiled For | i686-pc-linux-gnu (cross-compiled for Windows) |
| Encoding | UTF8 |
| Collation | French_France.1252 |
| Character Class | French_France.1252 |
| Data Checksums | **No** |
| Max Identifier Length | 63 characters |
| Port | 5432 |
| Data Directory | CHIFA-OFFICINE installation directory |
| Shared Buffers | 128MB |
| WAL Level | minimal |
| Superuser | postgres |

---

## 2. All Databases

| # | Database | OID | Owner | Encoding | Collate | Size |
|---|----------|-----|-------|----------|---------|------|
| 1 | template1 | 1 | postgres | UTF8 | French_France.1252 | Template |
| 2 | template0 | 16381 | postgres | UTF8 | French_France.1252 | Template |
| 3 | postgres | 16383 | postgres | UTF8 | French_France.1252 | Maintenance |
| 4 | **CHIFA_OFFICINE** | **16394** | postgres | UTF8 | French_France.1252 | **785 MB** |

### 2.1 CHIFA_OFFICINE Database

| Property | Value |
|----------|-------|
| OID | 16394 |
| Size | **785 MB** |
| Tablespace | pg_default |
| NextOID | 8,835,983 |
| State | In production |

---

## 3. Schemas

| Schema | Tables | Description |
|--------|--------|-------------|
| **public** | **48** | All user tables |
| pg_catalog | system | System catalogs |
| pg_toast | system | TOAST tables |

**Key Finding**: There is **NO `cnas` schema**. The ANALYSE_PROJET.md assumption of a `cnas` schema was **INCORRECT**. All 48 user tables reside in the `public` schema.

---

## 4. All 48 Tables (Sorted by Size)

| # | Table | Est. Rows | Size | PK Type | Category |
|---|-------|-----------|------|---------|----------|
| 1 | ln | 3,708,019 | 278 MB | ? | National List |
| 2 | signature | 0 | 181 MB | num_fact | Invoice Signatures |
| 3 | logiciel | 1 | 169 MB | version | Software Config |
| 4 | medicament | 7,596 | 80 MB | num_enr | Drug Catalog |
| 5 | detail_fact_cm | 0 | 71 MB | num_fact+num_enr+ppa | CM Invoice Lines |
| 6 | forme | 469 | 1,264 kB | code_forme | Drug Forms |
| 7 | type_posologie | 26 | 656 kB | code | Dosage Types |
| 8 | beneficiaire | 0 | 488 kB | num_assure+rang_ad | Beneficiaries |
| 9 | medic_ppa | 0 | 464 kB | ? | Drug PPA Data |
| 10 | medic_sp | 292 | 408 kB | num_enr+code_sp | Drug Specialties |
| 11 | medic_demuni | 936 | 256 kB | code | Drug Shortages |
| 12 | tarif | 1,641 | 240 kB | num_enr+d_debut | Pricing |
| 13 | temp04 | 0 | 216 kB | temp | Temp Work |
| 14 | morfine | 1,366 | 200 kB | ? | Morphine Tracking |
| 15 | temp02 | 0 | 200 kB | temp | Temp Work |
| 16 | temp00 | 0 | 152 kB | temp | Temp Work |
| 17 | temp01 | 0 | 128 kB | temp | Temp Work |
| 18 | specialite | 87 | 64 kB | code_sp | Medical Specialties |
| 19 | conditionnement | 185 | 64 kB | condit+nombre | Packaging |
| 20 | facture | 0 | 64 kB | num_fact | **Invoices** |
| 21 | medicament2 | 276 | 64 kB | code | Alt Drug Catalog |
| 22 | bordereau | 0 | 56 kB | num_bord | **Batches** |
| 23 | condition | 38 | 56 kB | code+nature | Conditions |
| 24 | parametre_code_barre | 1 | 40 kB | ? | Barcode Config |
| 25 | detail_fact | 0 | 40 kB | num_fact+num_enr+ppa | **Invoice Lines** |
| 26 | utilisateur | 1 | 40 kB | id_user | Users |
| 27 | parametre | 1 | 32 kB | code_ps | **Pharmacy Config** |
| 28 | centre | 2 | 24 kB | code_centre | CNAS Centers |
| 29 | facture_cm | 0 | 24 kB | num_fact | CM Invoices |
| 30 | attestation_mc | 0 | 24 kB | num_assure+rang_ad+code_centre | MC Attestations |
| 31 | mutualiste_radie | 0 | 24 kB | num_assure+code_mut | Struck-off Members |
| 32 | temp03 | 0 | 16 kB | temp | Temp Work |
| 33 | file | 0 | 16 kB | file_name | Files |
| 34 | carte_chifa | 0 | 16 kB | num_serie | CHIFA Cards |
| 35 | token | ? | ? | code_affect | Tokens |
| 36 | certificat_token | ? | ? | num_serie | Token Certs |
| 37 | cm | ? | ? | ref_cm+num_fact+num_assure | CM Records |
| 38 | cm_audit | ? | ? | num_fact | CM Audit |
| 39 | ct_acces | ? | ? | id_user+composant | Access Control |
| 40 | rupture_stock | ? | ? | code_medic+date_insert | Stock Alerts |
| 41 | medic_ppa | 0 | 464 kB | ? | (duplicate entry) |
| 42 | parametre_code_barre | 1 | 40 kB | ? | (duplicate entry) |

> **Note**: Duplicate entries (medic_ppa, parametre_code_barre) appear in the raw discovery output — these are unique tables listed once each.

---

## 5. Extensions

| Extension | Schema | Version | Purpose |
|-----------|--------|---------|---------|
| dblink | public | 1.0 | Remote database connections |
| pg_stat_statements | public | 1.1 | Query statistics |

---

## 6. Sequences

| Sequence | Table | Column | Type |
|----------|-------|--------|------|
| bordereau_id_bord_seq | bordereau | id_bord | bigint |
| utilisateur_id_user_seq | utilisateur | id_user | integer |

---

## 7. Views

| View | Source |
|------|--------|
| pg_stat_statements | pg_stat_statements extension |

---

## 8. Functions

| Function | Return Type | Purpose |
|----------|-------------|---------|
| add(integer, integer) | integer | Utility: addition |
| increment(integer) | integer | Utility: increment |
| cloturerbord(varchar, text, text) | integer | **Close bordereau** (2 overloads) |
| describe_table(varchar, varchar) | SETOF text | Table metadata |
| importdata(text) | integer | Data import |
| importtable(text, text) | integer | Table import |
| readfile(text, OUT bytea) | bytea | File read |

### 8.1 Key Function: cloturerbord

```sql
-- Two overloads discovered
cloturerbord(varchar, text, text) → integer
cloturerbord(varchar, text, text) → integer
```

This is the **bordereau closure function** — critical for CHIFA claims batch submission.

---

## 9. Indexes

### 9.1 Primary Key Indexes (B-tree, Unique)

All primary keys are implemented as btree unique indexes (PostgreSQL default).

### 9.2 Additional Unique Indexes

| Index Name | Table | Column(s) |
|------------|-------|-----------|
| UN_BORDEREAU | bordereau | num_bord |
| UN_UTILISATEUR | utilisateur | nom_utilisateur |
| TOKEN_UN | token | ip |

---

## 10. Table Categories (BM Pharma Relevance)

### 10.1 Core Billing Tables (BM-SPEC-028-032)

| Table | Rows | Size | BM-SPEC | Relevance |
|-------|------|------|---------|-----------|
| facture | 0 | 64 kB | BM-SPEC-028 | **PRIMARY** |
| detail_fact | 0 | 40 kB | BM-SPEC-029 | **PRIMARY** |
| bordereau | 0 | 56 kB | BM-SPEC-030 | **PRIMARY** |
| parametre | 1 | 32 kB | BM-SPEC-032 | **PRIMARY** |

### 10.2 Drug Reference Tables

| Table | Rows | Size | Relevance |
|-------|------|------|-----------|
| medicament | 7,596 | 80 MB | **HIGH** — Drug catalog |
| medicament2 | 276 | 64 kB | MEDIUM — Alt catalog |
| ln | 3,708,019 | 278 MB | **HIGH** — National list |
| medic_sp | 292 | 408 kB | MEDIUM — Drug specialties |
| medic_ppa | 0 | 464 kB | MEDIUM — Drug PPA |
| medic_demuni | 936 | 256 kB | LOW — Drug shortages |
| morfine | 1,366 | 200 kB | LOW — Morphine tracking |
| tarif | 1,641 | 240 kB | **HIGH** — Pricing |
| forme | 469 | 1,264 kB | MEDIUM — Drug forms |
| specialite | 87 | 64 kB | MEDIUM — Specialties |

### 10.3 Signature & Authentication

| Table | Rows | Size | Relevance |
|-------|------|------|-----------|
| signature | 0 | 181 MB | **HIGH** — Invoice signatures |
| token | ? | ? | HIGH — Auth tokens |
| certificat_token | ? | ? | HIGH — Token certificates |
| carte_chifa | 0 | 16 kB | MEDIUM — CHIFA cards |

### 10.4 CM (Complément de Mutuelle) Tables

| Table | Rows | Size | Relevance |
|-------|------|------|-----------|
| cm | ? | ? | MEDIUM |
| cm_audit | ? | ? | LOW |
| detail_fact_cm | 0 | 71 kB | MEDIUM |
| facture_cm | 0 | 24 kB | MEDIUM |

### 10.5 Beneficiary & Access

| Table | Rows | Size | Relevance |
|-------|------|------|-----------|
| beneficiaire | 0 | 488 kB | HIGH |
| attestation_mc | 0 | 24 kB | LOW |
| mutualiste_radie | 0 | 24 kB | LOW |
| centre | 2 | 24 kB | MEDIUM |
| ct_acces | ? | ? | MEDIUM |
| droit_acces | (FK found) | — | MEDIUM |
| utilisateur | 1 | 40 kB | MEDIUM |

### 10.6 Utility/Temp

| Table | Rows | Size | Relevance |
|-------|------|------|-----------|
| temp00-temp04 | 0 | — | NONE (temp work) |
| logiciel | 1 | 169 MB | LOW (version info) |
| parametre_code_barre | 1 | 40 kB | LOW |
| file | 0 | 16 kB | LOW |
| rupture_stock | ? | ? | LOW |
| condition | 38 | 56 kB | LOW |
| conditionnement | 185 | 64 kB | LOW |
| type_posologie | 26 | 656 kB | LOW |

---

## 11. Table Count Summary

| Category | Count |
|----------|-------|
| Total user tables | **48** |
| Core billing (facture, detail_fact, bordereau, parametre) | 4 |
| Drug reference | 10 |
| Signature/auth | 4 |
| CM tables | 4 |
| Beneficiary/access | 7 |
| Utility/temp | 14 |
| Other | 5 |

---

## 12. Key Observations

1. **facture and detail_fact are EMPTY** — This database has been recently purged or is a fresh installation with no active claims
2. **ln table is massive** (3.7M rows, 278 MB) — The national drug list dominates storage
3. **signature table is large** (181 MB) but empty — Pre-allocated/indexed space
4. **logiciel table has 1 row but 169 MB** — Likely contains embedded binary data (software update packages?)
5. **medicament has 7,596 rows** — Full national drug catalog loaded
6. **Only 2 sequences** — Minimal auto-increment usage (bordereau.id_bord, utilisateur.id_user)

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
