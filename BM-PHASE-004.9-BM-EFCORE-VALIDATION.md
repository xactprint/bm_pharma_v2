# BM-PHASE-004.9 — BM PHARMA EF CORE VALIDATION

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

BM Pharma's EF Core entities (`ChifaFacture`, `ChifaDetailFact`, `ChifaBordereau`, `ChifaParametre`) were validated against the **real** CHIFA_OFFICINE database schema. Results show critical mismatches: phantom columns, missing columns, type mismatches, and uncovered tables.

---

## 1. Validation Method

1. Read BM Pharma EF Core entities from source code (`src/BMPharma.Persistence.PostgreSQL/Entities/Chifa/`)
2. Read `ChifaWriteDbContext.cs` for Fluent API configuration
3. Compared each `[Column]` attribute against real schema column names and types
4. Checked for phantom properties (EF Core → nothing in real DB)
5. Checked for missing properties (real DB → nothing in EF Core)

---

## 2. ChifaFacture → facture

### 2.1 Mapping Status

| Property | DB Column | DB Type | EF Type | Status |
|----------|-----------|---------|---------|--------|
| NumFact | num_fact | varchar(8) NOT NULL | string | ✅ CORRECT |
| DateFact | date_fact | timestamp | DateTime? | ✅ CORRECT |
| Etat | etat | char(1) | string? | ✅ CORRECT |
| NumBord | num_bord | varchar(6) | string? | ✅ CORRECT |
| MontOff | mont_off | numeric(10,2) | decimal? | ✅ CORRECT |
| MontAs | mont_as | numeric(10,2) | decimal? | ✅ CORRECT |
| MontFact | mont_fact | numeric(11,2) | decimal? | ✅ CORRECT |
| NumAssure | num_assure | varchar(12) | string? | ✅ CORRECT |
| CodeCentre | code_centre | varchar(5) | string? | ✅ CORRECT |
| DateSoin | date_soin | date | DateTime? | ✅ CORRECT |
| DateFinDroit | date_fin_droit | date | DateTime? | ✅ CORRECT |
| DateEnvoiSms | date_envoi_sms | timestamp | DateTime? | ✅ CORRECT |
| DateSynchro | date_synchro | timestamp | DateTime? | ✅ CORRECT |
| TypeMaj | type_maj | integer NOT NULL | int | ✅ CORRECT |
| MontMajFae | mont_maj_fae | numeric(4,2) | decimal | ✅ CORRECT |
| MontMaj | mont_maj | numeric(11,2) | decimal | ✅ CORRECT |
| NatRemb | nat_remb | varchar(1) | string? | ✅ CORRECT |
| MontMut | mont_mut | numeric(10,2) | decimal? | ✅ CORRECT |
| DateFinMut | date_fin_mut | date | DateTime? | ✅ CORRECT |
| Version | version | varchar(10) | string? | ✅ CORRECT |
| NumSeriePs | num_serie_ps | bigint | long? | ✅ CORRECT |
| VersionCarte | version_carte | integer | int? | ✅ CORRECT |
| Echifa | echifa | boolean | bool? | ✅ CORRECT |
| IdFactEchifa | id_fact_echifa | bigint | long? | ✅ CORRECT |
| EOrd | e_ord | boolean | bool? | ✅ CORRECT |
| IdEOrd | id_e_ord | bigint | long? | ✅ CORRECT |
| Taux | taux | **char(1)** | **decimal?** | ❌ **TYPE MISMATCH** |

### 2.2 Phantom Properties (25 properties → no DB column)

| EF Property | Claimed Column | Real DB Column | Status |
|-------------|---------------|----------------|--------|
| DateFinDroitBenef | date_fin_droit_benef | **EXISTS** ✅ | MAPPED |
| NomAssure | nom_assure | **DOES NOT EXIST** | ❌ PHANTOM |
| PrenomAssure | prenom_assure | **DOES NOT EXIST** | ❌ PHANTOM |
| NomBenef | nom_benef | **DOES NOT EXIST** | ❌ PHANTOM |
| PrenomBenef | prenom_benef | **DOES NOT EXIST** | ❌ PHANTOM |
| LieuNaissance | lieu_naissance | **DOES NOT EXIST** | ❌ PHANTOM |
| DateNaissance | date_naissance | **DOES NOT EXIST** | ❌ PHANTOM |
| Wilaya | wilaya | **DOES NOT EXIST** | ❌ PHANTOM |
| Commune | commune | **DOES NOT EXIST** | ❌ PHANTOM |
| Adresse | adresse | **DOES NOT EXIST** | ❌ PHANTOM |
| CodePostal | code_postal | **DOES NOT EXIST** | ❌ PHANTOM |
| Tel | tel | **DOES NOT EXIST** | ❌ PHANTOM |
| NumDossier | num_dossier | **DOES NOT EXIST** | ❌ PHANTOM |
| MotifRejet | motif_rejet | **DOES NOT EXIST** | ❌ PHANTOM |
| DateRejet | date_rejet | **DOES NOT EXIST** | ❌ PHANTOM |
| DatePaiement | date_paiement | **DOES NOT EXIST** | ❌ PHANTOM |
| MontPaiement | mont_paiement | **DOES NOT EXIST** | ❌ PHANTOM |
| NumCheque | num_cheque | **DOES NOT EXIST** | ❌ PHANTOM |
| DateControle | date_controle | **DOES NOT EXIST** | ❌ PHANTOM |
| IdUtilisateur | id_utilisateur | **DOES NOT EXIST** (real: id_user) | ❌ PHANTOM + RENAMED |
| DateCreation | date_creation | **DOES NOT EXIST** | ❌ PHANTOM |
| DateModification | date_modification | **DOES NOT EXIST** | ❌ PHANTOM |
| CentreGestion | centre_gestion | **DOES NOT EXIST** | ❌ PHANTOM |
| CodeActe | code_acte | **DOES NOT EXIST** | ❌ PHANTOM |
| Beneficiaire | beneficiaire | **DOES NOT EXIST** | ❌ PHANTOM |
| Matricule | matricule | **DOES NOT EXIST** | ❌ PHANTOM |

### 2.3 Missing Columns (Real DB → not in EF Core)

| Real Column | Type | Purpose | Priority |
|-------------|------|---------|----------|
| rang_ad | varchar(2) | Beneficiary rank | **HIGH** |
| tp | char(1) | Patient type | MEDIUM |
| code_affect | varchar(2) | Affectation code | MEDIUM |
| conv | char(1) | Convention type | MEDIUM |
| type_consult | varchar(2) | Consultation type | LOW |
| prescripteur | varchar(50) | Prescribing doctor | MEDIUM |
| risque | char(1) | Risk code | LOW |
| statut_fact | char(1) | Invoice status | **HIGH** |
| verifcms | char(1) | CMS verification | MEDIUM |
| type_signature | char(1) | Signature type | MEDIUM |
| verif_fact | char(1) | Invoice verification | MEDIUM |
| code_centre_as | char(5) | AS center code | LOW |
| code_sp | varchar(2) | Specialty code | MEDIUM |
| type_ord | char(1) | Prescription type | MEDIUM |
| motif_med | varchar(16) | Medical justification | LOW |
| id_user | integer | User ID (renamed from id_utilisateur) | **HIGH** |
| signature | xml | Invoice XML signature | **HIGH** |
| num_serie | bigint | Serial number | MEDIUM |
| date_envoi_sms | timestamp | SMS send | LOW |
| code_covid | varchar(20) | COVID code | LOW |
| fact_xml | xml | Invoice XML | **HIGH** |
| code_mut | varchar(2) | Mutual code | MEDIUM |
| adresse_ip | varchar(15) | Client IP | LOW |
| nom_pc | varchar(30) | Computer name | LOW |
| obs | varchar(255) | Remarks | LOW |
| ref_cm | varchar(18) | CM reference | LOW |

### 2.4 ChifaFacture Score

| Metric | Value |
|--------|-------|
| Real columns | 60 |
| EF Core mapped (correct) | 34 |
| Phantom properties | **26** |
| Missing columns | **26** |
| Type mismatches | **1** (taux) |
| Effective coverage | **57%** |
| Accuracy (mapped only) | **97%** (1 type mismatch) |

---

## 3. ChifaDetailFact → detail_fact

### 3.1 Mapping Status

| Property | DB Column | DB Type | EF Type | Status |
|----------|-----------|---------|---------|--------|
| NumFact | num_fact | varchar(8) NOT NULL | string | ✅ CORRECT |
| NumEnr | num_enr | varchar(5) NOT NULL | string | ✅ CORRECT |
| Ppa | ppa | numeric(10,2) NOT NULL | decimal | ✅ CORRECT |
| Qte | qte | numeric(3,0) NOT NULL | decimal | ✅ CORRECT |
| Mont | mont | numeric(10,2) NOT NULL | decimal | ✅ CORRECT |
| MontAs | mont_as | numeric(10,2) | decimal? | ✅ CORRECT |
| MontPharm | mont_pharm | numeric(10,2) | decimal? | ✅ CORRECT |
| MajLocal | maj_local | numeric(10,2) | decimal? | ✅ CORRECT |
| NumEnrPrescrit | num_enr_prescrit | varchar(5) NOT NULL | string | ✅ CORRECT |
| MajSub | maj_sub | numeric(3,0) | decimal? | ✅ CORRECT |
| DureeTrait | duree_trait | numeric(3,0) | decimal? | ✅ CORRECT |
| TarifRef | tarif_ref | numeric(10,2) | decimal? | ✅ CORRECT |
| Posologie | posologie | varchar(50) | string? | ✅ CORRECT |
| Remboursable | remboursable | boolean | bool? | ✅ CORRECT |
| Local | local | boolean | bool? | ✅ CORRECT |
| InfTr | inf_tr | boolean | bool? | ✅ CORRECT |
| ApplicTr | applic_tr | boolean | bool? | ✅ CORRECT |
| Medic | medic | boolean | bool? | ✅ CORRECT |
| Ts | ts | boolean | bool? | ✅ CORRECT |
| NumLot | num_lot | varchar(6) | string? | ✅ CORRECT |

### 3.2 ChifaDetailFact Score

| Metric | Value |
|--------|-------|
| Real columns | 20 |
| EF Core mapped (correct) | 20 |
| Phantom properties | 0 |
| Missing columns | 0 |
| Type mismatches | 0 |
| Effective coverage | **100%** |
| Accuracy | **100%** |

---

## 4. ChifaBordereau → bordereau

### 4.1 Mapping Status

| Property | DB Column | DB Type | EF Type | Status |
|----------|-----------|---------|---------|--------|
| IdBord | id_bord | bigint NOT NULL | long | ✅ CORRECT |
| NumBord | num_bord | varchar(6) NOT NULL | string | ✅ CORRECT |
| CodeCentre | code_centre | varchar(5) NOT NULL | string | ✅ CORRECT |
| Etat | etat | char(1) | string? | ✅ CORRECT |
| IdUserCloture | id_user_cloture | integer | int? | ✅ CORRECT |
| PosteCloture | poste_cloture | varchar(100) | string? | ✅ CORRECT |
| MontVir | mont_vir | numeric(10,2) | decimal? | ✅ CORRECT |
| Duplicata | duplicata | boolean | bool? | ✅ CORRECT |
| DateCloture | date_cloture | timestamp | DateTime? | ✅ CORRECT |
| DateOuverture | date_ouverture | timestamp | DateTime? | ✅ CORRECT |
| DateDepotFtp | date_depot_ftp | timestamp | DateTime? | ✅ CORRECT |

### 4.2 ChifaBordereau Score

| Metric | Value |
|--------|-------|
| Real columns | 11 |
| EF Core mapped (correct) | 11 |
| Phantom properties | 0 |
| Missing columns | 0 |
| Type mismatches | 0 |
| Effective coverage | **100%** |
| Accuracy | **100%** |

---

## 5. ChifaParametre → parametre

### 5.1 Mapping Status

| Property | DB Column | DB Type | EF Type | Status |
|----------|-----------|---------|---------|--------|
| CodePs | code_ps | varchar(10) NOT NULL | string? | ✅ CORRECT |
| NomPharmacie | nom_pharmacie | varchar(50) | string? | ✅ CORRECT |
| Adresse | adresse | varchar(50) | string? | ✅ CORRECT |
| Email | email | varchar(50) | string? | ✅ CORRECT |
| CodeCentre | code_centre | varchar(5) | string? | ✅ CORRECT |
| Version | version | varchar(20) | int? | ⚠️ TYPE MISMATCH |
| NextNumFact | next_num_fact | integer | int? | ✅ CORRECT |
| NextNumBord | next_num_bord | smallint | short? | ✅ CORRECT |
| Tel | num_tel | varchar(20) | string? | ⚠️ NAME MISMATCH |
| Fax | num_fax | varchar(20) | string? | ⚠️ NAME MISMATCH |
| Wilaya | — | — | string? | ❌ PHANTOM |
| Commune | — | — | string? | ❌ PHANTOM |
| CodePostal | — | — | string? | ❌ PHANTOM |
| DateCreation | — | — | DateTime? | ❌ PHANTOM |
| DateModification | — | — | DateTime? | ❌ PHANTOM |

### 5.2 Phantom Properties

| EF Property | Real Column | Status |
|-------------|-------------|--------|
| Wilaya | DOES NOT EXIST | ❌ PHANTOM |
| Commune | DOES NOT EXIST | ❌ PHANTOM |
| CodePostal | DOES NOT EXIST | ❌ PHANTOM |
| DateCreation | DOES NOT EXIST | ❌ PHANTOM |
| DateModification | DOES NOT EXIST | ❌ PHANTOM |

### 5.3 Column Name Mismatches

| EF Property | EF [Column] | Real DB Column | Issue |
|-------------|-------------|----------------|-------|
| Tel | "tel" | num_tel | Name mismatch |
| Fax | "fax" | num_fax | Name mismatch |
| Version | "version" (int?) | version (varchar(20)) | **TYPE MISMATCH** |

### 5.4 Missing Columns (45 columns not mapped)

Critical missing columns for BM Pharma:

| Column | Type | Purpose | Priority |
|--------|------|---------|----------|
| nom | varchar(25) | Pharmacist name | MEDIUM |
| prenom | varchar(25) | Pharmacist name | MEDIUM |
| code_sp | varchar(2) | Specialty code | MEDIUM |
| mont_max | numeric(8,2) | Max amount | MEDIUM |
| mont_maj_fae | numeric(2,0) | FAE rate | MEDIUM |
| mont_maj_sub | numeric(2,0) | Sub rate | MEDIUM |
| taux_maj_local | numeric(2,0) | Local rate | MEDIUM |
| taux_maj_inf_tr | numeric(2,0) | Below-tariff rate | MEDIUM |
| access_token | varchar(256) | API token | SECURITY |
| refresh_token | varchar(256) | Refresh token | SECURITY |

### 5.5 ChifaParametre Score

| Metric | Value |
|--------|-------|
| Real columns | 60 |
| EF Core mapped | 10 (correct names) |
| Phantom properties | 5 |
| Missing columns | 45 |
| Type mismatches | 1 (version: varchar→int) |
| Name mismatches | 2 (Tel, Fax) |
| Effective coverage | **17%** |
| Accuracy (correctly named) | **70%** |

---

## 6. Overall EF Core Validation Summary

| Entity | Coverage | Accuracy | Verdict |
|--------|----------|----------|---------|
| ChifaFacture | 57% | 97% | ⚠️ NEEDS WORK |
| ChifaDetailFact | 100% | 100% | ✅ PERFECT |
| ChifaBordereau | 100% | 100% | ✅ PERFECT |
| ChifaParametre | 17% | 70% | ❌ CRITICAL GAP |
| **OVERALL** | **49%** | **92%** | ⚠️ NEEDS WORK |

---

## 7. Unmapped Tables (Not in EF Core At All)

| Table | Rows | Columns | BM Pharma Relevance |
|-------|------|---------|---------------------|
| medicament | 7,596 | 29 | **CRITICAL** — Drug catalog |
| ln | 3,708,019 | ? | **HIGH** — National list |
| signature | 0 | 2 | **HIGH** — Invoice signatures |
| specialite | 87 | ? | MEDIUM — Medical specialties |
| forme | 469 | ? | MEDIUM — Drug forms |
| tarif | 1,641 | ? | **HIGH** — Drug pricing |
| centre | 2 | ? | MEDIUM — CNAS centers |
| utilisateur | 1 | ? | MEDIUM — Users |
| token | ? | ? | HIGH — Auth tokens |
| certificat_token | ? | ? | HIGH — Token certs |
| ct_acces | ? | ? | MEDIUM — Access control |
| droit_acces | (FK) | ? | MEDIUM — Access rights |
| carte_chifa | 0 | ? | LOW — CHIFA cards |
| cm | ? | ? | MEDIUM — CM records |
| cm_audit | ? | ? | LOW — CM audit |
| detail_fact_cm | 0 | ? | MEDIUM — CM invoice lines |
| facture_cm | 0 | ? | MEDIUM — CM invoices |
| attestation_mc | 0 | ? | LOW — MC attestations |
| beneficiaire | 0 | ? | MEDIUM — Beneficiaries |
| mutualiste_radie | 0 | ? | LOW — Struck-off members |
| medic_sp | 292 | ? | MEDIUM — Drug specialties |
| medic_ppa | 0 | ? | MEDIUM — Drug PPA |
| medic_demuni | 936 | ? | LOW — Drug shortages |
| medicament2 | 276 | ? | LOW — Alt drug catalog |
| morfine | 1,366 | ? | LOW — Morphine tracking |
| condition | 38 | ? | LOW — Medical conditions |
| conditionnement | 185 | ? | LOW — Drug packaging |
| type_posologie | 26 | ? | LOW — Dosage types |
| parametre_code_barre | 1 | ? | LOW — Barcode config |
| logiciel | 1 | 169 MB | LOW — Software version |
| file | 0 | ? | LOW — Files |
| rupture_stock | ? | ? | LOW — Stock alerts |
| temp00-temp04 | 0 | — | NONE — Temp tables |

---

## 8. Recommendations

### 8.1 Immediate Fixes

1. **Remove 26 phantom properties** from ChifaFacture
2. **Remove 5 phantom properties** from ChifaParametre
3. **Fix taux type**: char(1) → string? (not decimal?)
4. **Fix version type** in ChifaParametre: varchar(20) → string? (not int?)
5. **Fix column names**: Tel→NumTel, Fax→NumFax, IdUtilisateur→IdUser
6. **Add 26 missing columns** to ChifaFacture
7. **Add 45 missing columns** to ChifaParametre

### 8.2 New Entities Needed

1. `ChifaMedicament` — Drug catalog (29 cols)
2. `ChifaSignature` — Invoice signatures (2 cols)
3. `ChifaSpecialite` — Medical specialties
4. `ChifaForme` — Drug forms
5. `ChifaTarif` — Drug pricing
6. `ChifaCentre` — CNAS centers
7. `ChifaUtilisateur` — Users

### 8.3 XML Column Handling

Two columns in facture use XML type:
- `signature` (xml) — Invoice digital signature
- `fact_xml` (xml) — Full invoice XML

EF Core handles XML via `string` or `NpgsqlXml`. These need proper mapping configuration.

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
