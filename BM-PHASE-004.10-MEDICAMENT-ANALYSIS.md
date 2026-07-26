# BM-PHASE-004.10 — MEDICAMENT TABLE ANALYSIS

## Status: COMPLETE

**Date**: 2026-07-26
**Phase**: BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment

---

## Executive Summary

The `medicament` table is the national drug catalog used by CHIFA-OFFICINE for invoice line validation, tariff reference lookup, reimbursement rate calculation, and generic substitution support. It contains 7,596 drugs across 29 columns, occupying 80 MB on disk. This is a NEW entity created in BM-PHASE-004.10.

---

## 1. Table Overview

| Property | Value |
|----------|-------|
| **Table Name** | medicament |
| **Schema** | public |
| **Rows** | 7,596 |
| **Physical Size** | 80 MB |
| **Average Row Size** | ~10 KB (TOAST compression on observation column) |
| **PK** | num_enr (varchar(5)) |
| **FK** | code_forme → forme.code_forme |
| **Entity** | ChifaMedicament |
| **EF Core Coverage** | 29/29 (100%) |

---

## 2. Complete Column Inventory

| # | Column | PG Type | Nullable | C# Property | C# Type | BM Pharma Relevance |
|---|--------|---------|----------|-------------|---------|---------------------|
| 1 | **num_enr** | varchar(5) | NOT NULL | NumEnr | string | **CRITICAL** — Drug ID, FK from detail_fact.num_enr |
| 2 | **nom_com** | varchar(50) | nullable | NomCom | string? | **HIGH** — Commercial name for display |
| 3 | **nom_dci** | varchar(60) | nullable | NomDci | string? | **HIGH** — Active ingredient name for search |
| 4 | **dosage** | varchar(30) | nullable | Dosage | string? | **HIGH** — Dosage for display |
| 5 | unite | varchar(20) | nullable | Unite | string? | MEDIUM — Unit of measure |
| 6 | conditionnement | varchar(20) | nullable | Conditionnement | string? | MEDIUM — Packaging info |
| 7 | convention | char(1) | nullable | Convention | string? | MEDIUM — Convention type |
| 8 | **remboursable** | char(1) | nullable | Remboursable | string? | **HIGH** — Reimbursement eligibility flag |
| 9 | date_remboursement | varchar(10) | nullable | DateRemboursement | string? | MEDIUM — Reimbursement start date |
| 10 | date_arret_remboursement | varchar(10) | nullable | DateArretRemboursement | string? | MEDIUM — Reimbursement end date |
| 11 | date_decision | varchar(10) | nullable | DateDecision | string? | LOW — Decision date |
| 12 | **tarif_ref** | numeric(11,2) | nullable | TarifRef | decimal? | **CRITICAL** — Reference tariff for pricing |
| 13 | **taux** | numeric(3,0) | nullable | Taux | decimal? | **CRITICAL** — Reimbursement rate (%) |
| 14 | code_forme | varchar(3) | nullable | CodeForme | string? | MEDIUM — FK to forme table |
| 15 | tableau | char(1) | nullable | Tableau | string? | LOW — Table code |
| 16 | hopital | char(1) | nullable | Hopital | string? | LOW — Hospital flag |
| 17 | secteur_sanitaire | char(1) | nullable | SecteurSanitaire | string? | LOW — Health sector |
| 18 | officine | char(1) | nullable | Officine | string? | LOW — Office flag |
| 19 | pays | varchar(20) | nullable | Pays | string? | LOW — Country of origin |
| 20 | laboratoire | varchar(25) | nullable | Laboratoire | string? | MEDIUM — Laboratory/manufacturer |
| 21 | cm | char(1) | nullable | Cm | string? | MEDIUM — CM (Complement Mutuelle) flag |
| 22 | code_medic | varchar(11) | nullable | CodeMedic | string? | MEDIUM — Drug code |
| 23 | date_tr | varchar(10) | nullable | DateTr | string? | MEDIUM — Tariff date |
| 24 | observation | varchar(2000) | nullable | Observation | string? | LOW — Notes/comments (causes TOAST) |
| 25 | code_dci | varchar(6) | nullable | CodeDci | string? | MEDIUM — DCI code |
| 26 | code_sp | varchar(2) | nullable | CodeSp | string? | MEDIUM — Specialty code |
| 27 | inf_tr | char(1) | nullable | InfTr | string? | MEDIUM — Below tariff flag |
| 28 | **generic** | char(1) | nullable | Generic | string? | **HIGH** — Generic flag for substitution |
| 29 | medic | char(1) | nullable | Medic | string? | LOW — Medication flag |

---

## 3. Critical Columns for BM Pharma

### 3.1 Pricing Chain

```
medicament.num_enr  ←── detail_fact.num_enr (FK)
medicament.tarif_ref ──→ detail_fact.tarif_ref (reference price)
medicament.taux     ──→ Reimbursement rate calculation
```

### 3.2 Drug Identification

| Column | Purpose | Example |
|--------|---------|---------|
| num_enr | Unique drug ID (PK) | "12345" |
| nom_com | Commercial name | "DOLIPRANE 1000MG" |
| nom_dci | Active ingredient | "PARACETAMOL" |
| dosage | Dosage form | "1000MG" |
| code_forme | Drug form code | "TAB" (links to forme table) |

### 3.3 Reimbursement Logic

| Column | Purpose | Values |
|--------|---------|--------|
| remboursable | Eligible for reimbursement | "O" (Oui), "N" (Non) |
| taux | Reimbursement rate (%) | 0-100 |
| tarif_ref | Reference price (DA) | e.g., 50.00 |
| generic | Generic flag | "O" (generic), "N" (original) |

---

## 4. BM Pharma ↔ CHIFA Medicament Mapping

### 4.1 Current BM Pharma Product Model

BM Pharma currently uses a local SQLite `products` table:

| BM Pharma Field | Type | Purpose |
|----------------|------|---------|
| Id | int | Auto-increment PK |
| Name | string | Product name |
| Description | string | Product description |
| Price | decimal | Selling price |
| Category | string | Product category |
| StockQuantity | int | Current stock |
| Barcode | string | Product barcode |

### 4.2 Mapping Matrix

| BM Pharma Field | CHIFA Medicament Field | Mapping | Notes |
|----------------|----------------------|---------|-------|
| Id | num_enr | MANUAL | CHIFA uses varchar(5), BM Pharma uses int |
| Name | nom_com | DIRECT | Commercial name |
| Description | nom_dci + dosage + conditionnement | COMPOSITE | Combine active ingredient + dosage + packaging |
| Price | tarif_ref | DIRECT | Reference tariff (may differ from selling price) |
| Category | code_forme | LOOKUP | Via forme table |
| StockQuantity | — | NO MAPPING | Stock is BM Pharma local concept |
| Barcode | code_medic | PARTIAL | Drug code (not a real barcode) |

### 4.3 Additional CHIFA Fields (Not in BM Pharma)

| CHIFA Field | Purpose | BM Pharma Use Case |
|-------------|---------|-------------------|
| taux | Reimbursement rate | Display reimbursement info |
| remboursable | Reimbursement eligibility | Filter reimbursable drugs |
| generic | Generic flag | Generic substitution support |
| laboratoire | Manufacturer | Display manufacturer info |
| convention | Convention type | Billing logic |
| code_dci | DCI code | Search by active ingredient |
| code_sp | Specialty code | Filter by medical specialty |

---

## 5. Foreign Key Relationships

| FK Column | References | Table | Relationship |
|-----------|-----------|-------|-------------|
| code_forme | code_forme | forme | Many-to-one (drug form) |
| code_sp | code_sp | specialite | Many-to-one (medical specialty) |
| num_enr | num_enr | detail_fact | One-to-many (invoice lines) |
| num_enr | num_enr | tarif | One-to-many (price history) |
| num_enr | num_enr | medic_sp | One-to-many (drug specialties) |

---

## 6. Data Characteristics

### 6.1 Size Analysis

| Metric | Value |
|--------|-------|
| Total Size | 80 MB |
| Row Count | 7,596 |
| Logical Row Size | ~80 bytes |
| Physical Row Size | ~10,500 bytes |
| TOAST Factor | ~130x |
| TOAST Columns | observation (varchar(2000)) |

### 6.2 Why So Large?

The `observation` column (varchar(2000)) likely contains detailed drug descriptions, usage instructions, contraindications, and other free-text data. PostgreSQL TOASTs (compresses) this data inline, causing the 80 MB physical size despite only 7,596 rows.

### 6.3 Recommended Access Patterns

| Pattern | Recommendation |
|---------|---------------|
| Lookup by num_enr | Use PK index (fast) |
| Search by nom_com | Consider adding GIN/trigram index |
| Filter by remboursable | Full table scan acceptable (7,596 rows) |
| Bulk load | DO NOT load all 80 MB into memory at once |
| Caching | Cache in-memory for frequent lookups (7,596 rows ≈ 2-5 MB in memory) |

---

## 7. Real DB Data (Sample)

| Field | Value |
|-------|-------|
| Total drugs | 7,596 |
| Table size | 80 MB |
| PK type | varchar(5) |
| Last updated | Via parametre.date_medicament |

---

## 8. Integration Recommendations

| # | Recommendation | Priority |
|---|---------------|----------|
| 1 | Load medicament table into local SQLite cache for offline use | HIGH |
| 2 | Build drug search UI using nom_com, nom_dci, dosage | HIGH |
| 3 | Implement tariff validation: detail_fact.ppa vs medicament.tarif_ref | HIGH |
| 4 | Add reimbursement rate display using medicament.taux | MEDIUM |
| 5 | Support generic substitution using medicament.generic flag | MEDIUM |
| 6 | Consider read-only caching of medicament (7,596 rows fits in memory) | LOW |

---

*Document generated by BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment*
