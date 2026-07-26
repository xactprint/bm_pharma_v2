# BM-PHASE-004.9 — NEW TABLES ANALYSIS

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

The real CHIFA_OFFICINE database contains **48 user tables**. BM-SPEC-028 through BM-SPEC-032 documented only **4 tables** (facture, detail_fact, bordereau, parametre). This document analyzes the **44 NEW tables** discovered, categorized by purpose and BM Pharma relevance.

---

## 1. Table Classification

| Category | Count | Tables | BM Pharma Relevance |
|----------|-------|--------|---------------------|
| **Core Billing** | 4 | facture, detail_fact, bordereau, parametre | PRIMARY |
| **Drug Reference** | 10 | medicament, medicament2, ln, medic_sp, medic_ppa, medic_demuni, morfine, tarif, forme, specialite | HIGH |
| **Signature & Auth** | 4 | signature, token, certificat_token, carte_chifa | HIGH |
| **CM (Complément Mutuelle)** | 4 | cm, cm_audit, detail_fact_cm, facture_cm | MEDIUM |
| **Beneficiary & Identity** | 3 | beneficiaire, attestation_mc, mutualiste_radie | MEDIUM |
| **Access Control** | 3 | utilisateur, ct_acces, droit_acces | MEDIUM |
| **Center & Condition** | 3 | centre, condition, conditionnement | LOW |
| **Packaging & Dosage** | 2 | type_posologie, parametre_code_barre | LOW |
| **Utility** | 2 | file, rupture_stock | LOW |
| **Temp Tables** | 5 | temp00, temp01, temp02, temp03, temp04 | NONE |
| **Software** | 1 | logiciel | NONE |

---

## 2. Drug Reference Tables (10 tables)

### 2.1 medicament — Drug Catalog

| Property | Value |
|----------|-------|
| Rows | 7,596 |
| Size | 80 MB |
| PK | num_enr (varchar(5)) |
| Purpose | Complete national drug catalog |
| Columns | 29 (see BM-PHASE-004.9-CORE-TABLES.md) |
| BM Pharma Relevance | **CRITICAL** — Referenced by detail_fact.num_enr |

**Key Columns**:
- `num_enr` — Drug enrollment number (links to detail_fact)
- `nom_com` — Commercial name (display in UI)
- `nom_dci` — Active ingredient name
- `tarif_ref` — Reference tariff (pricing)
- `taux` — Reimbursement rate
- `remboursable` — Reimbursement eligibility
- `generic` — Generic flag (substitution support)
- `code_forme` — Drug form (links to forme table)

**BM Pharma Impact**: Without mapping this table, BM Pharma cannot:
- Validate drug numbers during invoice creation
- Display drug names alongside invoice lines
- Verify reimbursement rates
- Support generic substitution

### 2.2 ln — National List

| Property | Value |
|----------|-------|
| Rows | **3,708,019** |
| Size | **278 MB** |
| PK | ? |
| Purpose | National drug reimbursement list (Liste Nationale) |
| BM Pharma Relevance | **HIGH** — Reference data for drug pricing |

**Analysis**: The largest table by row count. Contains historical national drug reimbursement data. Likely updated periodically from CNAS. Not directly referenced by facture/detail_fact but essential for drug validation.

### 2.3 tarif — Drug Pricing History

| Property | Value |
|----------|-------|
| Rows | 1,641 |
| Size | 240 kB |
| PK | (num_enr, d_debut) |
| Purpose | Historical drug pricing per enrollment number |
| BM Pharma Relevance | **HIGH** — Price validation |

**FK**: num_enr → medicament.num_enr

**Analysis**: Tracks drug price changes over time. `d_debut` is the effective date. Used to validate that the `ppa` in detail_fact matches the current tariff.

### 2.4 forme — Drug Forms

| Property | Value |
|----------|-------|
| Rows | 469 |
| Size | 1,264 kB |
| PK | code_forme (varchar(3)) |
| Purpose | Drug form lookup (tablet, syrup, etc.) |
| BM Pharma Relevance | MEDIUM — Display only |

### 2.5 specialite — Medical Specialties

| Property | Value |
|----------|-------|
| Rows | 87 |
| Size | 64 kB |
| PK | code_sp (varchar(2)) |
| Purpose | Medical specialty codes (cardiology, etc.) |
| BM Pharma Relevance | MEDIUM — Referenced by facture.code_sp |

### 2.6 medic_sp — Drug Specialties

| Property | Value |
|----------|-------|
| Rows | 292 |
| Size | 408 kB |
| PK | (num_enr, code_sp) |
| Purpose | Links drugs to medical specialties |
| BM Pharma Relevance | MEDIUM |

### 2.7 medic_ppa — Drug PPA Data

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 464 kB |
| PK | ? |
| Purpose | Drug PPA (Prix de Prestation Annexe) data |
| BM Pharma Relevance | MEDIUM — Pricing data |

### 2.8 medic_demuni — Drug Shortages

| Property | Value |
|----------|-------|
| Rows | 936 |
| Size | 256 kB |
| PK | code |
| Purpose | Drug shortage/unavailability tracking |
| BM Pharma Relevance | LOW |

### 2.9 medicament2 — Alternative Drug Catalog

| Property | Value |
|----------|-------|
| Rows | 276 |
| Size | 64 kB |
| PK | code |
| Purpose | Secondary drug catalog (possibly older format) |
| BM Pharma Relevance | LOW |

### 2.10 morfine — Morphine Tracking

| Property | Value |
|----------|-------|
| Rows | 1,366 |
| Size | 200 kB |
| PK | ? |
| Purpose | Controlled substance (morphine) tracking |
| BM Pharma Relevance | LOW — Regulatory compliance |

---

## 3. Signature & Authentication Tables (4 tables)

### 3.1 signature — Invoice Signatures

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 181 MB |
| PK | num_fact (varchar(8)) |
| Purpose | Digital signature for each invoice |
| Columns | num_fact, sign (text) |
| BM Pharma Relevance | **CRITICAL** — Required for CHIFA submission |

**FK**: num_fact → facture.num_fact

**Analysis**: Empty but pre-allocated to 181 MB. This table stores the cryptographic signature of each invoice for CNAS submission. The large size suggests heavy indexing or LOB pre-allocation.

### 3.2 token — Authentication Tokens

| Property | Value |
|----------|-------|
| Rows | ? |
| Size | ? |
| PK | code_affect |
| Purpose | Authentication token storage |
| Unique Index | TOKEN_UN on (ip) |
| BM Pharma Relevance | HIGH — Auth flow |

### 3.3 certificat_token — Token Certificates

| Property | Value |
|----------|-------|
| Rows | ? |
| Size | ? |
| PK | num_serie |
| Purpose | Smart card token certificates |
| BM Pharma Relevance | HIGH — Smart card auth |

### 3.4 carte_chifa — CHIFA Cards

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 16 kB |
| PK | num_serie (bigint) |
| Purpose | CHIFA beneficiary card registration |
| BM Pharma Relevance | MEDIUM — Card management |

---

## 4. CM (Complément Mutuelle) Tables (4 tables)

### 4.1 cm — CM Records

| Property | Value |
|----------|-------|
| Rows | ? |
| Size | ? |
| PK | (ref_cm, num_fact, num_assure) |
| Purpose | Complément Mutuelle claim records |
| BM Pharma Relevance | MEDIUM |

### 4.2 cm_audit — CM Audit

| Property | Value |
|----------|-------|
| Rows | ? |
| Size | ? |
| PK | num_fact |
| Purpose | CM audit trail |
| BM Pharma Relevance | LOW |

### 4.3 detail_fact_cm — CM Invoice Lines

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 71 MB |
| PK | (num_fact, num_enr, ppa) |
| Purpose | Complément Mutuelle invoice line items |
| BM Pharma Relevance | MEDIUM |

### 4.4 facture_cm — CM Invoices

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 24 kB |
| PK | num_fact |
| Purpose | Complément Mutuelle invoice headers |
| BM Pharma Relevance | MEDIUM |

**FK**: facture_cm.num_bord → bordereau.num_bord

---

## 5. Beneficiary & Identity Tables (3 tables)

### 5.1 beneficiaire — Beneficiaries

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 488 kB |
| PK | (num_assure, rang_ad) |
| Purpose | Beneficiary identity information |
| BM Pharma Relevance | MEDIUM — Beneficiary validation |

### 5.2 attestation_mc — MC Attestations

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 24 kB |
| PK | (num_assure, rang_ad, code_centre) |
| Purpose | Medical certification attestations |
| BM Pharma Relevance | LOW |

### 5.3 mutualiste_radie — Struck-off Members

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 24 kB |
| PK | (num_assure, code_mut) |
| Purpose | Mutual members removed from coverage |
| BM Pharma Relevance | LOW |

---

## 6. Access Control Tables (3 tables)

### 6.1 utilisateur — Users

| Property | Value |
|----------|-------|
| Rows | 1 |
| Size | 40 kB |
| PK | id_user (integer, auto-increment) |
| Unique | UN_UTILISATEUR on (nom_utilisateur) |
| Purpose | User accounts |
| BM Pharma Relevance | MEDIUM — Audit trail |

### 6.2 ct_acces — Access Control

| Property | Value |
|----------|-------|
| Rows | ? |
| PK | (id_user, composant) |
| Purpose | User component access permissions |
| BM Pharma Relevance | MEDIUM — RBAC |

### 6.3 droit_acces — Access Rights

| Property | Value |
|----------|-------|
| Rows | (FK to utilisateur) |
| PK | (id_user, composant) |
| Purpose | Detailed access rights per user |
| FK | id_user → utilisateur.id_user |
| BM Pharma Relevance | MEDIUM — RBAC |

---

## 7. Center & Condition Tables (3 tables)

### 7.1 centre — CNAS Centers

| Property | Value |
|----------|-------|
| Rows | 2 |
| Size | 24 kB |
| PK | code_centre (varchar(5)) |
| Purpose | CNAS office/center codes |
| BM Pharma Relevance | MEDIUM — Referenced by facture.code_centre |

### 7.2 condition — Medical Conditions

| Property | Value |
|----------|-------|
| Rows | 38 |
| Size | 56 kB |
| PK | (code, nature) |
| Purpose | Medical condition codes |
| BM Pharma Relevance | LOW |

### 7.3 conditionnement — Drug Packaging

| Property | Value |
|----------|-------|
| Rows | 185 |
| Size | 64 kB |
| PK | (condit, nombre) |
| Purpose | Drug packaging configurations |
| BM Pharma Relevance | LOW |

---

## 8. Packaging & Dosage Tables (2 tables)

### 8.1 type_posologie — Dosage Types

| Property | Value |
|----------|-------|
| Rows | 26 |
| Size | 656 kB |
| PK | code |
| Purpose | Dosage instruction types |
| BM Pharma Relevance | LOW |

### 8.2 parametre_code_barre — Barcode Config

| Property | Value |
|----------|-------|
| Rows | 1 |
| Size | 40 kB |
| PK | ? |
| Purpose | Barcode scanning configuration |
| BM Pharma Relevance | LOW |

---

## 9. Utility Tables (2 tables)

### 9.1 file — Files

| Property | Value |
|----------|-------|
| Rows | 0 |
| Size | 16 kB |
| PK | file_name |
| Purpose | File storage reference |
| BM Pharma Relevance | LOW |

### 9.2 rupture_stock — Stock Alerts

| Property | Value |
|----------|-------|
| Rows | ? |
| PK | (code_medic, date_insert) |
| Purpose | Drug stock shortage alerts |
| BM Pharma Relevance | LOW |

---

## 10. Temp Tables (5 tables)

| Table | Size | Purpose |
|-------|------|---------|
| temp00 | 152 kB | Temporary work data |
| temp01 | 128 kB | Temporary work data |
| temp02 | 200 kB | Temporary work data |
| temp03 | 16 kB | Temporary work data |
| temp04 | 216 kB | Temporary work data |

All temp tables are empty (0 rows). These are used by CHIFA-OFFICINE for intermediate processing and can be ignored for BM Pharma integration.

---

## 11. Software Table (1 table)

### 11.1 logiciel — Software Version

| Property | Value |
|----------|-------|
| Rows | 1 |
| Size | 169 MB |
| PK | version |
| Purpose | Software version data (likely embedded binary) |
| BM Pharma Relevance | NONE |

The 169 MB size for 1 row strongly suggests embedded binary data — possibly the CHIFA-OFFICINE installer or update package stored as a BLOB/TEXT.

---

## 12. Priority Matrix for BM Pharma Integration

### Tier 1: CRITICAL (Must Map)

| Table | Reason |
|-------|--------|
| medicament | Drug catalog — needed for invoice line validation |
| signature | Invoice signatures — needed for CHIFA submission |

### Tier 2: HIGH (Should Map)

| Table | Reason |
|-------|--------|
| ln | National drug list — pricing validation |
| tarif | Drug pricing history — price validation |
| token | Auth tokens — authentication flow |
| certificat_token | Token certificates — smart card auth |

### Tier 3: MEDIUM (Nice to Have)

| Table | Reason |
|-------|--------|
| specialite | Medical specialties — display |
| forme | Drug forms — display |
| centre | CNAS centers — display |
| utilisateur | Users — audit trail |
| ct_acces | Access control — RBAC |
| droit_acces | Access rights — RBAC |
| cm | CM records — complement mutuelle |
| detail_fact_cm | CM invoice lines — complement mutuelle |
| facture_cm | CM invoices — complement mutuelle |
| beneficiaire | Beneficiaries — display |
| medic_sp | Drug specialties — validation |
| medic_ppa | Drug PPA data — pricing |

### Tier 4: LOW (Ignore for Now)

| Table | Reason |
|-------|--------|
| carte_chifa | Card management — not needed |
| attestation_mc | MC attestations — not needed |
| mutualiste_radie | Struck-off members — not needed |
| cm_audit | CM audit — not needed |
| medic_demuni | Drug shortages — not needed |
| medicament2 | Alt drug catalog — not needed |
| morfine | Morphine tracking — not needed |
| condition | Medical conditions — not needed |
| conditionnement | Drug packaging — not needed |
| type_posologie | Dosage types — not needed |
| parametre_code_barre | Barcode config — not needed |
| file | File storage — not needed |
| rupture_stock | Stock alerts — not needed |
| logiciel | Software version — not needed |
| temp00-temp04 | Temp tables — not needed |

---

## 13. Summary

| Metric | Value |
|--------|-------|
| Total user tables | 48 |
| Tables in BM-SPEC | 4 |
| NEW tables discovered | **44** |
| Critical (Tier 1) | 2 |
| High (Tier 2) | 4 |
| Medium (Tier 3) | 12 |
| Low (Tier 4) | 15 |
| Temp/Ignore | 6 |
| Already mapped (BM-SPEC) | 4 |
| **New entities needed** | **~18** |

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
