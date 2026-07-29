# BM-PHASE-010 — DATABASE CONTRACT

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phase 004.9 (real discovery), Phase 004.10 (entity correction), Phase 004.12 (final validation)
**Previous Version:** BM_PHARMA_CHIFA_DATABASE_CONTRACT.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial contract based on BM-SPEC-028-032 and Docker test schema |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with real CHIFA_OFFICINE PostgreSQL (48 tables, 9.3.4 32-bit) |

### Major Changes from v1.0

| Change | v1.0 (Hypothesis) | v2.0 (Real) | Source |
|--------|-------------------|-------------|--------|
| Connection username | `pharm` (assumed) | `pharm` (confirmed superuser) | Phase 004.11 |
| Schema | `cnas` (assumed) | `public` (confirmed) | Phase 004.9 |
| PostgreSQL version | 9.3.x (documented) | 9.3.4 32-bit (confirmed) | Phase 004.12 |
| Tables | 4 (contract only) | 48 (discovered) | Phase 004.9 |
| facture columns (entity) | 40 (25 phantom) | 53 (0 phantom) | Phase 004.10 |
| parametre columns (entity) | 15 (5 phantom) | 58 (0 phantom) | Phase 004.10 |
| Entities | 4 | 6 (+medicament, +signature) | Phase 004.10 |
| EF Core coverage | 49% | 96% | Phase 004.10 |

---

## Overview

This document defines the technical contract between BM Pharma and the real CHIFA_OFFICINE PostgreSQL database.

BM Pharma accesses CHIFA tables through Npgsql 8.x / EF Core 8. The CHIFA database is READ-WRITE but BM Pharma's default mode is READ-ONLY.

---

## Connection Configuration

| Parameter | Value | Status |
|-----------|-------|--------|
| Host | 127.0.0.1 (localhost) | CONFIRMED |
| Port | 5432 | CONFIRMED |
| Database | CHIFA_OFFICINE | CONFIRMED |
| Username | pharm | CONFIRMED |
| Password | (empty — trust auth) | CONFIRMED |
| SSL Mode | Disable | CONFIRMED |
| Schema | public | CONFIRMED |
| PostgreSQL | 9.3.4 (32-bit, EOL 2018) | CONFIRMED |
| Trust Server Certificate | true | CONFIRMED |
| Timeout | 10s | CONFIRMED |
| Command Timeout | 30s | CONFIRMED |
| Pooling | true | CONFIRMED |
| Max Pool Size | 5 | CONFIRMED |

### Connection String (Production)

```
Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=;SslMode=Disable;TrustServerCertificate=true;Timeout=10;CommandTimeout=30;Pooling=true;Max Pool Size=5
```

### Authentication Model

| Aspect | Detail | Status |
|--------|--------|--------|
| Auth method | trust (no password required) | CONFIRMED |
| Security | Acceptable for localhost embedded PG only | CONFIRMED |
| User role | `pharm` = superuser, database owner | CONFIRMED |
| `postgres` user | No CONNECT privilege initially; GRANT issued in Phase 004.11 | CONFIRMED |

---

## Database Inventory

### Discovered Tables: 48 (Phase 004.9)

BM Pharma interacts with these critical tables:

| Table | Columns | Rows (real) | BM Pharma Access | Status |
|-------|---------|-------------|-----------------|--------|
| facture | 53 | 0 | READ + WRITE | CONFIRMED |
| detail_fact | 20 | 0 | READ + WRITE | CONFIRMED |
| bordereau | 11 | 0 | READ (+ bordereau INSERT possible but not recommended) | CONFIRMED |
| parametre | 58 | 1 (singleton) | READ | CONFIRMED |
| medicament | 29 | 7,596 | READ | CONFIRMED |
| signature | 2 | 0 | READ (INSERT not possible — requires CHIFA token) | CONFIRMED |
| ln | 9 | 7,412,276 | READ | CONFIRMED |
| 42 other tables | — | — | READ (discovered but not mapped) | CONFIRMED |

---

## Core Tables — EF Core Mappings

### 1. facture (53 columns) — Entity: ChifaFacture

**PK:** num_fact (varchar(8), NOT NULL)

**Critical columns for BM Pharma writes:**

| Column | Type | Nullable | Default | Entity Property | Status |
|--------|------|----------|---------|----------------|--------|
| num_fact | varchar(8) | NO | — | string NumFact [MaxLength(8)] | CONFIRMED |
| date_fact | timestamp | YES | — | DateTime? DateFact | CONFIRMED |
| etat | char(1) | YES | — | string? Etat | CONFIRMED |
| num_bord | varchar(6) | YES | — | string? NumBord | CONFIRMED |
| mont_off | numeric(10,2) | YES | — | decimal? MontOff | CONFIRMED |
| mont_as | numeric(10,2) | YES | — | decimal? MontAs | CONFIRMED |
| mont_fact | numeric(11,2) | YES | — | decimal? MontFact | CONFIRMED |
| num_assure | varchar(12) | YES | — | string? NumAssure | CONFIRMED |
| code_centre | varchar(5) | YES | — | string? CodeCentre | CONFIRMED |
| date_soin | date | YES | — | DateTime? DateSoin | CONFIRMED |
| type_maj | integer | NO | 0 | int TypeMaj | CONFIRMED |
| mont_maj_fae | numeric(4,2) | NO | 0 | decimal MontMajFae | CONFIRMED |
| mont_maj | numeric(11,2) | NO | 0 | decimal MontMaj | CONFIRMED |
| taux | numeric(5,2) | YES | — | decimal? Taux | CONFIRMED |
| signature | xml | YES | — | string? Signature | CONFIRMED (type: xml) |
| fact_xml | xml | YES | — | string? FactXml | CONFIRMED (type: xml) |

**Full 53-column mapping:** See Phase 004.8 compatibility matrix.
**Entity corrected:** Phase 004.10 (0 phantom properties, 53 real properties).

### 2. detail_fact (20 columns) — Entity: ChifaDetailFact

**PK:** (num_fact, num_enr, ppa) — composite

| Column | Type | Nullable | Entity Property | Status |
|--------|------|----------|----------------|--------|
| num_fact | varchar(8) | NO (PK1, FK) | string NumFact | CONFIRMED |
| num_enr | varchar(5) | NO (PK2) | string NumEnr | CONFIRMED |
| ppa | numeric(10,2) | NO (PK3) | decimal Ppa | CONFIRMED |
| qte | numeric(3,0) | NO | decimal Qte | CONFIRMED |
| mont | numeric(10,2) | NO | decimal Mont | CONFIRMED |
| mont_as | numeric(10,2) | YES | decimal? MontAs | CONFIRMED |
| mont_pharm | numeric(10,2) | YES | decimal? MontPharm | CONFIRMED |
| medic | boolean | YES | bool? Medic | CONFIRMED |
| ts | boolean | YES | bool? Ts | CONFIRMED |

**Full 20-column mapping:** See Phase 004.8.

### 3. bordereau (11 columns) — Entity: ChifaBordereau

**PK:** id_bord (bigint, auto-increment via sequence)
**Business PK (unique):** num_bord (varchar(6))

| Column | Type | Nullable | Entity Property | Status |
|--------|------|----------|----------------|--------|
| id_bord | bigint | NO (PK) | long IdBord | CONFIRMED |
| num_bord | varchar(6) | NO (unique) | string NumBord | CONFIRMED |
| code_centre | varchar(5) | NO | string CodeCentre | CONFIRMED |
| etat | char(1) | YES | string? Etat | CONFIRMED |
| date_ouverture | timestamp | YES | DateTime? DateOuverture | CONFIRMED |
| date_cloture | timestamp | YES | DateTime? DateCloture | CONFIRMED |
| date_depot_ftp | timestamp | YES | DateTime? DateDepotFtp | CONFIRMED |

### 4. parametre (58 columns) — Entity: ChifaParametre

**Key:** Single-row table (HasNoKey in EF Core)

| Column | Type | Current Value | Entity Property | Status |
|--------|------|--------------|----------------|--------|
| code_ps | varchar(10) | 1234567890 | string? CodePs | CONFIRMED |
| code_centre | varchar(5) | 11600 | string? CodeCentre | CONFIRMED |
| nom_pharmacie | varchar(50) | — | string? NomPharmacie | CONFIRMED |
| next_num_fact | integer | 1 | int? NextNumFact | CONFIRMED |
| next_num_bord | smallint | 215 | short? NextNumBord | CONFIRMED |

**Full 58-column mapping:** See Phase 004.10. Entity corrected (0 phantom properties).

### 5. medicament (29 columns) — Entity: ChifaMedicament

**PK:** num_enr (varchar(5))

| Column | Type | Entity Property | Status |
|--------|------|----------------|--------|
| num_enr | varchar(5) | string NumEnr | CONFIRMED |
| designation | varchar(200) | string? Designation | CONFIRMED |
| ppa | numeric(10,2) | decimal? Ppa | CONFIRMED |
| prix_vente | numeric(10,2) | decimal? PrixVente | CONFIRMED |
| remboursable | boolean | bool? Remboursable | CONFIRMED |

**Full 29-column mapping:** See Phase 004.10-MEDICAMENT-ANALYSIS.md.

### 6. signature (2 columns) — Entity: ChifaSignature

**PK:** num_fact (varchar(8))

| Column | Type | Entity Property | Status |
|--------|------|----------------|--------|
| num_fact | varchar(8) | string NumFact | CONFIRMED |
| signature | text | string? SignatureData | CONFIRMED |

---

## Write-Safe Tables

BM Pharma can write to these tables only:

| Table | Operations | Verified | Status |
|-------|-----------|----------|--------|
| facture | INSERT | Phase 007 (TST002) | CONFIRMED |
| detail_fact | INSERT | Phase 007 (TST002) | CONFIRMED |
| bordereau | INSERT | CONFIRMED possible but NOT RECOMMENDED (invisibility risk) | PARTIAL |
| parametre | UPDATE (counters only) | Via ChifaNumberingService | CONFIRMED |
| All other tables (42+) | READ-ONLY | Phase 004.9 | CONFIRMED |

---

## Constraints

### Primary Keys

| Table | Constraint | Column(s) | Status |
|-------|-----------|-----------|--------|
| facture | facture_pkey | num_fact | CONFIRMED |
| detail_fact | detail_fact_pkey | num_fact, num_enr, ppa | CONFIRMED |
| bordereau | bordereau_pkey | id_bord | CONFIRMED |
| medicament | medicament_pkey | num_enr | CONFIRMED |
| signature | signature_pkey | num_fact | CONFIRMED |

### Foreign Keys

| Source | Column | Target | Column | Status |
|--------|--------|--------|--------|--------|
| facture | num_bord | bordereau | num_bord | CONFIRMED |
| detail_fact | num_fact | facture | num_fact | CONFIRMED |

### Indexes

Total discovered: 34 indexes (Phase 004.12).
Critical indexes for BM Pharma:

| Table | Index | Type | Status |
|-------|-------|------|--------|
| bordereau | idx_bordereau_num_bord | UNIQUE | CONFIRMED |
| facture | idx_facture_num_bord | INDEX | CONFIRMED |

### Sequences

| Sequence | Table.Column | Status |
|----------|-------------|--------|
| bordereau_id_bord_seq | bordereau.id_bord (auto-increment) | CONFIRMED |

### Functions

Total discovered: 51 functions (Phase 004.12).
Critical function for BM Pharma:

| Function | Purpose | Status |
|----------|---------|--------|
| cloturerbord() | Bordereau closure (NOT callable by BM Pharma) | CONFIRMED (Phase 008) |

---

## Column Length Limits (Hard Constraints)

| Table.Column | Max Length | Validation | Status |
|-------------|-----------|-----------|--------|
| facture.num_fact | 8 chars | ChifaInvoiceValidator | CONFIRMED |
| facture.num_bord | 6 chars | ChifaBordereauValidator | CONFIRMED |
| facture.num_assure | 12 chars | ChifaInvoiceValidator | CONFIRMED |
| facture.code_centre | 5 chars | ChifaInvoiceValidator | CONFIRMED |
| detail_fact.num_enr | 5 chars | ChifaInvoiceValidator | CONFIRMED |
| detail_fact.num_lot | 6 chars | ChifaInvoiceValidator | CONFIRMED |
| detail_fact.qte | 999 (numeric(3,0)) | ChifaInvoiceValidator | CONFIRMED |
| bordereau.num_bord | 6 chars | ChifaBordereauValidator | CONFIRMED |

---

## Known Limitations

| Limitation | Detail | Status |
|-----------|--------|--------|
| PostgreSQL 9.3.4 EOL | No security patches since 2018 | CONFIRMED |
| PostgreSQL 32-bit | Max 2GB memory, crash-prone | CONFIRMED |
| Trust auth | No password required — security concern | CONFIRMED |
| Credentials in memory | CHIFA process memory contains plaintext PG credentials | CONFIRMED (Phase 009-C) |
| Tokens in parametre | Potential token values in plaintext | PARTIAL (suspected, not confirmed) |
| 7 dropped columns in facture | attnum gaps, not mapped | CONFIRMED |
| Npgsql 2.0.14.3 (CHIFA) vs 8.x (BM Pharma) | Separate DLLs, no conflict | CONFIRMED |
