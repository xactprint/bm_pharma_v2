# BM-PHASE-004.9 — CORE TABLES DEEP-DIVE

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

This document provides a column-by-column analysis of the 5 core tables most critical to BM Pharma integration: `facture` (60 cols), `detail_fact` (20 cols), `bordereau` (11 cols), `parametre` (60 cols), and `medicament` (29 cols).

---

## 1. facture — Invoice Header (60 columns)

### 1.1 Column Inventory

| # | Column | PG Type | Nullable | BM Pharma Entity | BM Spec | Status |
|---|--------|---------|----------|------------------|---------|--------|
| 1 | num_fact | varchar(8) | NOT NULL | NumFact | BM-SPEC-028 | ✅ MAPPED |
| 2 | date_fact | timestamp | nullable | DateFact | BM-SPEC-028 | ✅ MAPPED |
| 3 | etat | char(1) | nullable | Etat | BM-SPEC-028 | ✅ MAPPED |
| 4 | num_bord | varchar(6) | nullable | NumBord | BM-SPEC-028 | ✅ MAPPED |
| 5 | mont_off | numeric(10,2) | nullable | MontOff | BM-SPEC-028 | ✅ MAPPED |
| 6 | mont_as | numeric(10,2) | nullable | MontAs | BM-SPEC-028 | ✅ MAPPED |
| 7 | mont_fact | numeric(11,2) | nullable | MontFact | BM-SPEC-028 | ✅ MAPPED |
| 8 | num_assure | varchar(12) | nullable | NumAssure | BM-SPEC-028 | ✅ MAPPED |
| 9 | rang_ad | varchar(2) | nullable | — | BM-SPEC-028 | ❌ **MISSING** |
| 10 | code_centre | varchar(5) | nullable | CodeCentre | BM-SPEC-028 | ✅ MAPPED |
| 11 | tp | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 12 | taux | char(1) | nullable | Taux | — | ⚠️ TYPE MISMATCH |
| 13 | code_affect | varchar(2) | nullable | — | — | ❌ **NOT MAPPED** |
| 14 | conv | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 15 | type_consult | varchar(2) | nullable | — | — | ❌ **NOT MAPPED** |
| 16 | prescripteur | varchar(50) | nullable | — | — | ❌ **NOT MAPPED** |
| 17 | date_soin | date | nullable | DateSoin | BM-SPEC-028 | ✅ MAPPED |
| 18 | risque | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 19 | statut_fact | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 20 | verifcms | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 21 | type_signature | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 22 | verif_fact | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 23 | mont_maj_fae | numeric(4,2) | nullable | MontMajFae | BM-SPEC-028 | ✅ MAPPED |
| 24 | mont_maj | numeric(11,2) | nullable | MontMaj | BM-SPEC-028 | ✅ MAPPED |
| 25 | type_maj | integer | NOT NULL | TypeMaj | BM-SPEC-028 | ✅ MAPPED |
| 26 | code_centre_as | char(5) | nullable | — | — | ❌ **NOT MAPPED** |
| 27 | code_sp | varchar(2) | nullable | — | — | ❌ **NOT MAPPED** |
| 28 | type_ord | char(1) | nullable | — | — | ❌ **NOT MAPPED** |
| 29 | motif_med | varchar(16) | nullable | — | — | ❌ **NOT MAPPED** |
| 30 | id_user | integer | nullable | IdUtilisateur | — | ⚠️ NAME MISMATCH |
| 31 | signature | xml | nullable | — | — | ❌ **NOT MAPPED** (XML) |
| 32 | num_serie | bigint | nullable | — | — | ❌ **NOT MAPPED** |
| 33 | date_envoi_sms | timestamp | nullable | — | — | ❌ **NOT MAPPED** |
| — | *(gap: attnum 34-35)* | — | — | — | — | Column dropped |
| 36 | date_fin_droit | date | nullable | DateFinDroit | BM-SPEC-028 | ✅ MAPPED |
| 37 | date_fin_droit_benef | date | nullable | DateFinDroitBenef | — | ⚠️ PHANTOM |
| 38 | version | varchar(10) | nullable | Version | — | ✅ MAPPED |
| 39 | code_covid | varchar(20) | nullable | — | — | ❌ **NOT MAPPED** |
| — | *(gap: attnum 40)* | — | — | — | — | Column dropped |
| 41 | fact_xml | xml | nullable | — | — | ❌ **NOT MAPPED** (XML) |
| — | *(gap: attnum 42-45)* | — | — | — | — | Columns dropped |
| 46 | nat_remb | varchar(1) | nullable | NatRemb | — | ✅ MAPPED |
| 47 | mont_mut | numeric(10,2) | nullable | MontMut | — | ✅ MAPPED |
| 48 | date_fin_mut | date | nullable | DateFinMut | — | ✅ MAPPED |
| 49 | code_mut | varchar(2) | nullable | — | — | ❌ **NOT MAPPED** |
| 50 | date_synchro | timestamp | nullable | DateSynchro | — | ✅ MAPPED |
| 51 | adresse_ip | varchar(15) | nullable | — | — | ❌ **NOT MAPPED** |
| 52 | nom_pc | varchar(30) | nullable | — | — | ❌ **NOT MAPPED** |
| 53 | obs | varchar(255) | nullable | — | — | ❌ **NOT MAPPED** |
| 54 | ref_cm | varchar(18) | nullable | — | — | ❌ **NOT MAPPED** |
| 55 | num_serie_ps | bigint | nullable | NumSeriePs | — | ✅ MAPPED |
| 56 | version_carte | integer | nullable | VersionCarte | — | ✅ MAPPED |
| 57 | echifa | boolean | nullable | Echifa | — | ✅ MAPPED |
| 58 | id_fact_echifa | bigint | nullable | IdFactEchifa | — | ✅ MAPPED |
| 59 | e_ord | boolean | nullable | EOrd | — | ✅ MAPPED |
| 60 | id_e_ord | bigint | nullable | IdEOrd | — | ✅ MAPPED |

### 1.2 BM Pharma EF Core Phantom Columns

The `ChifaFacture` entity has **15 properties that DO NOT EXIST** in the real `facture` table:

| EF Core Property | DB Column | Status |
|-----------------|-----------|--------|
| NomAssure | nom_assure | ❌ **PHANTOM** |
| PrenomAssure | prenom_assure | ❌ **PHANTOM** |
| NomBenef | nom_benef | ❌ **PHANTOM** |
| PrenomBenef | prenom_benef | ❌ **PHANTOM** |
| LieuNaissance | lieu_naissance | ❌ **PHANTOM** |
| DateNaissance | date_naissance | ❌ **PHANTOM** |
| Wilaya | wilaya | ❌ **PHANTOM** |
| Commune | commune | ❌ **PHANTOM** |
| Adresse | adresse | ❌ **PHANTOM** |
| CodePostal | code_postal | ❌ **PHANTOM** |
| Tel | tel | ❌ **PHANTOM** |
| NumDossier | num_dossier | ❌ **PHANTOM** |
| MotifRejet | motif_rejet | ❌ **PHANTOM** |
| DateRejet | date_rejet | ❌ **PHANTOM** |
| DatePaiement | date_paiement | ❌ **PHANTOM** |
| MontPaiement | mont_paiement | ❌ **PHANTOM** |
| NumCheque | num_cheque | ❌ **PHANTOM** |
| DateControle | date_controle | ❌ **PHANTOM** |
| IdUtilisateur | id_utilisateur | ❌ **PHANTOM** (real col is `id_user`) |
| DateCreation | date_creation | ❌ **PHANTOM** |
| DateModification | date_modification | ❌ **PHANTOM** |
| CentreGestion | centre_gestion | ❌ **PHANTOM** |
| CodeActe | code_acte | ❌ **PHANTOM** |
| Beneficiaire | beneficiaire | ❌ **PHANTOM** |
| Matricule | matricule | ❌ **PHANTOM** |

### 1.3 Real Columns NOT in EF Core

| Real Column | Type | BM Pharma Need |
|-------------|------|----------------|
| rang_ad | varchar(2) | Needed for beneficiary identification |
| tp | char(1) | Patient type |
| code_affect | varchar(2) | Affectation code |
| conv | char(1) | Convention type |
| type_consult | varchar(2) | Consultation type |
| prescripteur | varchar(50) | Prescribing doctor |
| risque | char(1) | Risk code |
| statut_fact | char(1) | Invoice status |
| verifcms | char(1) | CMS verification |
| type_signature | char(1) | Signature type |
| verif_fact | char(1) | Invoice verification |
| code_centre_as | char(5) | AS center code |
| code_sp | varchar(2) | Specialty code |
| type_ord | char(1) | Prescription type |
| motif_med | varchar(16) | Medical justification |
| signature | xml | Invoice XML signature |
| num_serie | bigint | Serial number |
| date_envoi_sms | timestamp | SMS date |
| code_covid | varchar(20) | COVID code |
| fact_xml | xml | Invoice XML |
| code_mut | varchar(2) | Mutual code |
| adresse_ip | varchar(15) | Client IP |
| nom_pc | varchar(30) | Computer name |
| obs | varchar(255) | Remarks |
| ref_cm | varchar(18) | CM reference |

### 1.4 Taux Type Mismatch

| Property | Real Type | EF Core Type | Issue |
|----------|-----------|--------------|-------|
| taux | **char(1)** | decimal? (5,2) | **TYPE MISMATCH** — char(1) cannot be stored as decimal |

---

## 2. detail_fact — Invoice Lines (20 columns)

### 2.1 Column Inventory

| # | Column | PG Type | Nullable | BM Pharma Entity | Status |
|---|--------|---------|----------|------------------|--------|
| 1 | num_fact | varchar(8) | NOT NULL | NumFact | ✅ MAPPED |
| 2 | num_lot | varchar(6) | nullable | NumLot | ✅ MAPPED |
| 3 | num_enr | varchar(5) | NOT NULL | NumEnr | ✅ MAPPED |
| 4 | qte | numeric(3,0) | NOT NULL | Qte | ✅ MAPPED |
| 5 | ppa | numeric(10,2) | NOT NULL | Ppa | ✅ MAPPED |
| 6 | mont | numeric(10,2) | NOT NULL | Mont | ✅ MAPPED |
| 7 | mont_as | numeric(10,2) | nullable | MontAs | ✅ MAPPED |
| 8 | mont_pharm | numeric(10,2) | nullable | MontPharm | ✅ MAPPED |
| 9 | maj_local | numeric(10,2) | nullable | MajLocal | ✅ MAPPED |
| 10 | num_enr_prescrit | varchar(5) | NOT NULL | NumEnrPrescrit | ✅ MAPPED |
| 11 | maj_sub | numeric(3,0) | nullable | MajSub | ✅ MAPPED |
| 12 | duree_trait | numeric(3,0) | nullable | DureeTrait | ✅ MAPPED |
| 13 | tarif_ref | numeric(10,2) | nullable | TarifRef | ✅ MAPPED |
| 14 | posologie | varchar(50) | nullable | Posologie | ✅ MAPPED |
| 15 | remboursable | boolean | nullable | Remboursable | ✅ MAPPED |
| 16 | local | boolean | nullable | Local | ✅ MAPPED |
| 17 | inf_tr | boolean | nullable | InfTr | ✅ MAPPED |
| 18 | applic_tr | boolean | nullable | ApplicTr | ✅ MAPPED |
| 19 | medic | boolean | nullable | Medic | ✅ MAPPED |
| 20 | ts | boolean | nullable | Ts | ✅ MAPPED |

### 2.2 Mapping Result

**20/20 columns mapped** — 100% match with real schema.

**PK mapping**: Composite key `(num_fact, num_enr, ppa)` correctly configured in EF Core.

**Note**: `qte` is NOT NULL in the real schema but is NOT part of the PK. EF Core maps it as required (`decimal` not `decimal?`), which is correct.

---

## 3. bordereau — Batches (11 columns)

### 3.1 Column Inventory

| # | Column | PG Type | Nullable | BM Pharma Entity | Status |
|---|--------|---------|----------|------------------|--------|
| 1 | id_bord | bigint | NOT NULL | IdBord | ✅ MAPPED |
| 2 | num_bord | varchar(6) | NOT NULL | NumBord | ✅ MAPPED |
| 3 | code_centre | varchar(5) | NOT NULL | CodeCentre | ✅ MAPPED |
| 4 | etat | char(1) | nullable | Etat | ✅ MAPPED |
| 5 | id_user_cloture | integer | nullable | IdUserCloture | ✅ MAPPED |
| 6 | poste_cloture | varchar(100) | nullable | PosteCloture | ✅ MAPPED |
| 7 | mont_vir | numeric(10,2) | nullable | MontVir | ✅ MAPPED |
| 8 | duplicata | boolean | nullable | Duplicata | ✅ MAPPED |
| 9 | date_cloture | timestamp | nullable | DateCloture | ✅ MAPPED |
| 10 | date_ouverture | timestamp | nullable | DateOuverture | ✅ MAPPED |
| 11 | date_depot_ftp | timestamp | nullable | DateDepotFtp | ✅ MAPPED |

### 3.2 Mapping Result

**11/11 columns mapped** — 100% match with real schema.

**PK mapping**: `IdBord` (bigint) is the PK with sequence `bordereau_id_bord_seq`. The unique constraint `UN_BORDEREAU` on `num_bord` is NOT modeled in EF Core.

**FK mapping**: `facture.num_bord → bordereau.num_bord` is NOT modeled as a navigation property in EF Core.

---

## 4. parametre — Pharmacy Configuration (60 columns)

### 4.1 Column Inventory

| # | Column | PG Type | Nullable | BM Pharma Entity | Status |
|---|--------|---------|----------|------------------|--------|
| 1 | code_ps | varchar(10) | NOT NULL | CodePs | ✅ MAPPED |
| 2 | nom_pharmacie | varchar(50) | nullable | NomPharmacie | ✅ MAPPED |
| 3 | nom | varchar(25) | nullable | — | ❌ **NOT MAPPED** |
| 4 | prenom | varchar(25) | nullable | — | ❌ **NOT MAPPED** |
| 5 | adresse | varchar(50) | nullable | Adresse | ✅ MAPPED |
| 6 | num_tel | varchar(20) | nullable | Tel | ⚠️ NAME MISMATCH |
| 7 | num_fax | varchar(20) | nullable | Fax | ⚠️ NAME MISMATCH |
| 8 | email | varchar(50) | nullable | Email | ✅ MAPPED |
| 9 | code_sp | varchar(2) | nullable | — | ❌ **NOT MAPPED** |
| 10 | nis | varchar(15) | nullable | — | ❌ **NOT MAPPED** |
| 11 | nico | varchar(14) | nullable | — | ❌ **NOT MAPPED** |
| 12 | ndps | varchar(14) | nullable | — | ❌ **NOT MAPPED** |
| 13 | code_centre | varchar(5) | nullable | CodeCentre | ✅ MAPPED |
| 14 | convention | char(1) | nullable | — | ❌ **NOT MAPPED** |
| 15 | ref_convention | varchar(25) | nullable | — | ❌ **NOT MAPPED** |
| 16 | ref_bancaire | varchar(20) | nullable | — | ❌ **NOT MAPPED** |
| 17 | mode_reglement | char(1) | nullable | — | ❌ **NOT MAPPED** |
| 18 | mont_max | numeric(8,2) | nullable | — | ❌ **NOT MAPPED** |
| 19 | contact | char(40) | nullable | — | ❌ **NOT MAPPED** |
| 20 | mont_maj_fae | numeric(2,0) | nullable | — | ❌ **NOT MAPPED** |
| 21 | mont_maj_sub | numeric(2,0) | nullable | — | ❌ **NOT MAPPED** |
| 22 | taux_maj_local | numeric(2,0) | nullable | — | ❌ **NOT MAPPED** |
| 23 | taux_maj_inf_tr | numeric(2,0) | nullable | — | ❌ **NOT MAPPED** |
| 24 | version | varchar(20) | nullable | Version | ✅ MAPPED |
| 25 | date_medicament | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 26 | date_liste_noire | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 27 | date_liste_mc | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 28 | date_version | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 29 | date_specialite | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 30 | date_tarif | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 31 | date_note | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 32 | date_convention | varchar(10) | nullable | — | ❌ **NOT MAPPED** |
| 33 | nb_ord_max | integer | nullable | — | ❌ **NOT MAPPED** |
| 34 | officine_dgsn | boolean | nullable | — | ❌ **NOT MAPPED** |
| 35 | chemin_backup | varchar(200) | nullable | — | ❌ **NOT MAPPED** |
| 36 | heure_backup | varchar(5) | nullable | — | ❌ **NOT MAPPED** |
| 37 | nb_backup | integer | nullable | — | ❌ **NOT MAPPED** |
| 38 | next_num_fact | integer | nullable | NextNumFact | ✅ MAPPED |
| 39 | next_num_bord | smallint | nullable | NextNumBord | ✅ MAPPED |
| 40 | poste_serveur_chifa | boolean | nullable | — | ❌ **NOT MAPPED** |
| 41 | poste_telech | varchar(30) | nullable | — | ❌ **NOT MAPPED** |
| 44 | date_api_chifa | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 45 | date_verif_maj | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 46 | date_verif_cm | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 47 | date_mut_radie | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 48 | params | varchar(255) | nullable | — | ❌ **NOT MAPPED** |
| 49 | version_db | integer | nullable | — | ❌ **NOT MAPPED** |
| 50 | version_ftp | integer | nullable | — | ❌ **NOT MAPPED** |
| 51 | date_version_ftp | date | nullable | — | ❌ **NOT MAPPED** |
| 52 | annee | integer | nullable | — | ❌ **NOT MAPPED** |
| 53 | date_ln_complete | date | nullable | — | ❌ **NOT MAPPED** |
| 54 | backup_start | boolean | nullable | — | ❌ **NOT MAPPED** |
| 55 | backup_exit | boolean | nullable | — | ❌ **NOT MAPPED** |
| 56 | date_medicament2 | timestamp | nullable | — | ❌ **NOT MAPPED** |
| 57 | date_medic_ppa | date | nullable | — | ❌ **NOT MAPPED** |
| 58 | access_token | varchar(256) | nullable | — | ❌ **NOT MAPPED** |
| 59 | refresh_token | varchar(256) | nullable | — | ❌ **NOT MAPPED** |
| 60 | date_medic_demuni | timestamp | nullable | — | ❌ **NOT MAPPED** |

### 4.2 Mapping Result

**15/60 columns mapped** — 25% coverage.

**EF Core**: `ChifaParametre` is configured with `HasNoKey()` — this is acceptable since parametre is a singleton table (always 1 row).

**Column Name Mismatches**:
- Real: `num_tel` → EF Core: `Tel`
- Real: `num_fax` → EF Core: `Fax`

**Phantom Properties**: `Wilaya`, `Commune`, `CodePostal`, `DateCreation`, `DateModification` do NOT exist in the real `parametre` table.

**Critical Columns for BM Pharma**:
| Column | Purpose | In EF Core? |
|--------|---------|-------------|
| next_num_fact | Next invoice number generation | ✅ YES |
| next_num_bord | Next batch number generation | ✅ YES |
| code_ps | Pharmacy identifier | ✅ YES |
| nom_pharmacie | Pharmacy name | ✅ YES |
| access_token | API token (SECURITY RISK) | ❌ NO |
| refresh_token | API refresh token (SECURITY RISK) | ❌ NO |

---

## 5. medicament — Drug Catalog (29 columns)

### 5.1 Column Inventory

The `medicament` table is **NOT mapped** in BM Pharma's EF Core entities. This table is critical for:

- Drug name lookup during invoice creation
- Tariff reference validation
- Reimbursement rate determination
- Generic substitution support

| # | Column | Type | Purpose | BM Pharma Need |
|---|--------|------|---------|----------------|
| 1 | num_enr | varchar(5) | Drug ID (PK) | **CRITICAL** — FK from detail_fact |
| 2 | nom_com | varchar(50) | Commercial name | **HIGH** — Display |
| 3 | nom_dci | varchar(60) | Active ingredient | **HIGH** — Search |
| 4 | dosage | varchar(30) | Dosage | **HIGH** — Display |
| 5 | unite | varchar(20) | Unit | MEDIUM |
| 6 | conditionnement | varchar(20) | Packaging | MEDIUM |
| 7 | convention | char(1) | Convention type | MEDIUM |
| 8 | remboursable | char(1) | Reimbursable | **HIGH** — Logic |
| 9 | date_remboursement | varchar(10) | Reimbursement date | MEDIUM |
| 10 | date_arret_remboursement | varchar(10) | Stop date | MEDIUM |
| 11 | date_decision | varchar(10) | Decision date | LOW |
| 12 | tarif_ref | numeric(11,2) | Reference tariff | **CRITICAL** — Pricing |
| 13 | taux | numeric(3,0) | Reimbursement rate | **CRITICAL** — Calculation |
| 14 | code_forme | varchar(3) | Form code | MEDIUM |
| 15 | tableau | char(1) | Table code | LOW |
| 16 | hopital | char(1) | Hospital flag | LOW |
| 17 | secteur_sanitaire | char(1) | Health sector | LOW |
| 18 | officine | char(1) | Office flag | LOW |
| 19 | pays | varchar(20) | Country | LOW |
| 20 | laboratoire | varchar(25) | Laboratory | MEDIUM |
| 21 | cm | char(1) | CM flag | MEDIUM |
| 22 | code_medic | varchar(11) | Drug code | MEDIUM |
| 23 | date_tr | varchar(10) | Tariff date | MEDIUM |
| 24 | observation | varchar(2000) | Notes | LOW |
| 25 | code_dci | varchar(6) | DCI code | MEDIUM |
| 26 | code_sp | varchar(2) | Specialty code | MEDIUM |
| 27 | inf_tr | char(1) | Below tariff | MEDIUM |
| 28 | generic | char(1) | Generic flag | **HIGH** — Substitution |
| 29 | medic | char(1) | Medication flag | LOW |

### 5.2 Size Analysis

- **7,596 rows** × ~80 bytes avg = ~600 kB logical
- **80 MB physical** = significant TOAST compression overhead
- `observation` (varchar(2000)) likely contains large text data causing TOAST

---

## 6. Summary Matrix

| Table | Real Columns | EF Core Props | Mapped | Phantom | Coverage |
|-------|-------------|---------------|--------|---------|----------|
| facture | 60 | ~40 | 28 | 15 | **47%** |
| detail_fact | 20 | 20 | 20 | 0 | **100%** |
| bordereau | 11 | 11 | 11 | 0 | **100%** |
| parametre | 60 | 15 | 12 | 3 | **20%** |
| medicament | 29 | 0 | 0 | 0 | **0%** |
| **TOTAL** | **180** | **86** | **71** | **18** | **39%** |

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
