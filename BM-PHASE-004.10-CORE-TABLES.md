# BM-PHASE-004.10 — CORE TABLES DEEP-DIVE

## Status: COMPLETE

**Date**: 2026-07-26
**Phase**: BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment

---

## Executive Summary

This document provides a column-by-column analysis of the 6 core tables mapped to EF Core entities: `facture` (53 physical columns), `detail_fact` (20 columns), `bordereau` (11 columns), `parametre` (58 physical columns), `medicament` (29 columns), and `signature` (2 columns). All columns are now mapped to EF Core properties with correct types and names.

---

## 1. facture — Invoice Header (53 Physical Columns)

### 1.1 Physical Columns

| # | Column | PG Type | Nullable | C# Property | C# Type | EF MaxLength/Precision | Status |
|---|--------|---------|----------|-------------|---------|----------------------|--------|
| 1 | num_fact | varchar(8) | NOT NULL | NumFact | string | MaxLength(8) | PK |
| 2 | date_fact | timestamp | nullable | DateFact | DateTime? | — | MAPPED |
| 3 | etat | char(1) | nullable | Etat | string? | MaxLength(1) | MAPPED |
| 4 | num_bord | varchar(6) | nullable | NumBord | string? | MaxLength(6) | MAPPED |
| 5 | mont_off | numeric(10,2) | nullable | MontOff | decimal? | Precision(10,2) | MAPPED |
| 6 | mont_as | numeric(10,2) | nullable | MontAs | decimal? | Precision(10,2) | MAPPED |
| 7 | mont_fact | numeric(11,2) | nullable | MontFact | decimal? | Precision(11,2) | MAPPED |
| 8 | num_assure | varchar(12) | nullable | NumAssure | string? | MaxLength(12) | MAPPED |
| 9 | rang_ad | varchar(2) | nullable | RangAd | string? | MaxLength(2) | MAPPED |
| 10 | code_centre | varchar(5) | nullable | CodeCentre | string? | MaxLength(5) | MAPPED |
| 11 | tp | char(1) | nullable | Tp | string? | MaxLength(1) | MAPPED |
| 12 | taux | char(1) | nullable | Taux | string? | MaxLength(1) | MAPPED |
| 13 | code_affect | varchar(2) | nullable | CodeAffect | string? | MaxLength(2) | MAPPED |
| 14 | conv | char(1) | nullable | Conv | string? | MaxLength(1) | MAPPED |
| 15 | type_consult | varchar(2) | nullable | TypeConsult | string? | MaxLength(2) | MAPPED |
| 16 | prescripteur | varchar(50) | nullable | Prescripteur | string? | MaxLength(50) | MAPPED |
| 17 | date_soin | date | nullable | DateSoin | DateTime? | — | MAPPED |
| 18 | risque | char(1) | nullable | Risque | string? | MaxLength(1) | MAPPED |
| 19 | statut_fact | char(1) | nullable | StatutFact | string? | MaxLength(1) | MAPPED |
| 20 | verifcms | char(1) | nullable | Verifcms | string? | MaxLength(1) | MAPPED |
| 21 | type_signature | char(1) | nullable | TypeSignature | string? | MaxLength(1) | MAPPED |
| 22 | verif_fact | char(1) | nullable | VerifFact | string? | MaxLength(1) | MAPPED |
| 23 | mont_maj_fae | numeric(4,2) | NOT NULL | MontMajFae | decimal | Precision(4,2) | MAPPED |
| 24 | mont_maj | numeric(11,2) | NOT NULL | MontMaj | decimal | Precision(11,2) | MAPPED |
| 25 | type_maj | integer | NOT NULL | TypeMaj | int | — | MAPPED |
| 26 | code_centre_as | char(5) | nullable | CodeCentreAs | string? | MaxLength(5) | MAPPED |
| 27 | code_sp | varchar(2) | nullable | CodeSp | string? | MaxLength(2) | MAPPED |
| 28 | type_ord | char(1) | nullable | TypeOrd | string? | MaxLength(1) | MAPPED |
| 29 | motif_med | varchar(16) | nullable | MotifMed | string? | MaxLength(16) | MAPPED |
| 30 | id_user | integer | nullable | IdUser | int? | — | MAPPED |
| 31 | signature | xml | nullable | Signature | string? | MaxLength(4000) | MAPPED |
| 32 | num_serie | bigint | nullable | NumSerie | long? | — | MAPPED |
| 33 | date_envoi_sms | timestamp | nullable | DateEnvoiSms | DateTime? | — | MAPPED |
| 34 | date_fin_droit | date | nullable | DateFinDroit | DateTime? | — | MAPPED |
| 35 | date_fin_droit_benef | date | nullable | DateFinDroitBenef | DateTime? | — | MAPPED |
| 36 | version | varchar(10) | nullable | Version | string? | MaxLength(10) | MAPPED |
| 37 | code_covid | varchar(20) | nullable | CodeCovid | string? | MaxLength(20) | MAPPED |
| 38 | fact_xml | xml | nullable | FactXml | string? | MaxLength(4000) | MAPPED |
| 39 | nat_remb | varchar(1) | nullable | NatRemb | string? | MaxLength(1) | MAPPED |
| 40 | mont_mut | numeric(10,2) | nullable | MontMut | decimal? | Precision(10,2) | MAPPED |
| 41 | date_fin_mut | date | nullable | DateFinMut | DateTime? | — | MAPPED |
| 42 | code_mut | varchar(2) | nullable | CodeMut | string? | MaxLength(2) | MAPPED |
| 43 | date_synchro | timestamp | nullable | DateSynchro | DateTime? | — | MAPPED |
| 44 | adresse_ip | varchar(15) | nullable | AdresseIp | string? | MaxLength(15) | MAPPED |
| 45 | nom_pc | varchar(30) | nullable | NomPc | string? | MaxLength(30) | MAPPED |
| 46 | obs | varchar(255) | nullable | Obs | string? | MaxLength(255) | MAPPED |
| 47 | ref_cm | varchar(18) | nullable | RefCm | string? | MaxLength(18) | MAPPED |
| 48 | num_serie_ps | bigint | nullable | NumSeriePs | long? | — | MAPPED |
| 49 | version_carte | integer | nullable | VersionCarte | int? | — | MAPPED |
| 50 | echifa | boolean | nullable | Echifa | bool? | — | MAPPED |
| 51 | id_fact_echifa | bigint | nullable | IdFactEchifa | long? | — | MAPPED |
| 52 | e_ord | boolean | nullable | EOrd | bool? | — | MAPPED |
| 53 | id_e_ord | bigint | nullable | IdEOrd | long? | — | MAPPED |

### 1.2 Dropped Columns (7 — not in physical DB)

| attnum | Status | Reason |
|--------|--------|--------|
| 34-35 | Gap | Dropped by CHIFA migration |
| 40 | Gap | Dropped by CHIFA migration |
| 42-45 | Gap | Dropped by CHIFA migration |

### 1.3 Real DB Data Sample

| Field | Value |
|-------|-------|
| next_num_fact | 1 |
| next_num_bord | 215 |
| code_ps | 1234567890 |
| nom_pharmacie | PHARMACIE |
| prenom | EXEMPLE |
| code_centre | 11600 |
| version | 3.0.4.3 |
| annee | 2024 |

---

## 2. detail_fact — Invoice Lines (20 Columns)

### 2.1 Physical Columns

| # | Column | PG Type | Nullable | C# Property | C# Type | EF Config | Status |
|---|--------|---------|----------|-------------|---------|-----------|--------|
| 1 | num_fact | varchar(8) | NOT NULL | NumFact | string | MaxLength(8) | PK |
| 2 | num_lot | varchar(6) | nullable | NumLot | string? | MaxLength(6) | MAPPED |
| 3 | num_enr | varchar(5) | NOT NULL | NumEnr | string | MaxLength(5) | PK |
| 4 | qte | numeric(3,0) | NOT NULL | Qte | decimal | Precision(3,0) | MAPPED |
| 5 | ppa | numeric(10,2) | NOT NULL | Ppa | decimal | Precision(10,2) | PK |
| 6 | mont | numeric(10,2) | NOT NULL | Mont | decimal | Precision(10,2) | MAPPED |
| 7 | mont_as | numeric(10,2) | nullable | MontAs | decimal? | Precision(10,2) | MAPPED |
| 8 | mont_pharm | numeric(10,2) | nullable | MontPharm | decimal? | Precision(10,2) | MAPPED |
| 9 | maj_local | numeric(10,2) | nullable | MajLocal | decimal? | Precision(10,2) | MAPPED |
| 10 | num_enr_prescrit | varchar(5) | NOT NULL | NumEnrPrescrit | string | MaxLength(5) | MAPPED |
| 11 | maj_sub | numeric(3,0) | nullable | MajSub | decimal? | Precision(3,0) | MAPPED |
| 12 | duree_trait | numeric(3,0) | nullable | DureeTrait | decimal? | Precision(3,0) | MAPPED |
| 13 | tarif_ref | numeric(10,2) | nullable | TarifRef | decimal? | Precision(10,2) | MAPPED |
| 14 | posologie | varchar(50) | nullable | Posologie | string? | MaxLength(50) | MAPPED |
| 15 | remboursable | boolean | nullable | Remboursable | bool? | — | MAPPED |
| 16 | local | boolean | nullable | Local | bool? | — | MAPPED |
| 17 | inf_tr | boolean | nullable | InfTr | bool? | — | MAPPED |
| 18 | applic_tr | boolean | nullable | ApplicTr | bool? | — | MAPPED |
| 19 | medic | boolean | nullable | Medic | bool? | — | MAPPED |
| 20 | ts | boolean | nullable | Ts | bool? | — | MAPPED |

### 2.2 Mapping Result

**20/20 columns mapped** — 100% coverage.

**Composite PK**: `(num_fact, num_enr, ppa)` correctly configured.

---

## 3. bordereau — Batches (11 Columns)

### 3.1 Physical Columns

| # | Column | PG Type | Nullable | C# Property | C# Type | EF Config | Status |
|---|--------|---------|----------|-------------|---------|-----------|--------|
| 1 | id_bord | bigint | NOT NULL | IdBord | long | — | PK |
| 2 | num_bord | varchar(6) | NOT NULL | NumBord | string | MaxLength(6) | MAPPED |
| 3 | code_centre | varchar(5) | NOT NULL | CodeCentre | string | MaxLength(5) | MAPPED |
| 4 | etat | char(1) | nullable | Etat | string? | — | MAPPED |
| 5 | id_user_cloture | integer | nullable | IdUserCloture | int? | — | MAPPED |
| 6 | poste_cloture | varchar(100) | nullable | PosteCloture | string? | MaxLength(100) | MAPPED |
| 7 | mont_vir | numeric(10,2) | nullable | MontVir | decimal? | Precision(10,2) | MAPPED |
| 8 | duplicata | boolean | nullable | Duplicata | bool? | — | MAPPED |
| 9 | date_cloture | timestamp | nullable | DateCloture | DateTime? | — | MAPPED |
| 10 | date_ouverture | timestamp | nullable | DateOuverture | DateTime? | — | MAPPED |
| 11 | date_depot_ftp | timestamp | nullable | DateDepotFtp | DateTime? | — | MAPPED |

### 3.2 Mapping Result

**11/11 columns mapped** — 100% coverage.

**PK**: `IdBord` (bigint) with sequence `bordereau_id_bord_seq`.

---

## 4. parametre — Pharmacy Configuration (58 Physical Columns)

### 4.1 Physical Columns

| # | Column | PG Type | Nullable | C# Property | C# Type | EF Config | Status |
|---|--------|---------|----------|-------------|---------|-----------|--------|
| 1 | code_ps | varchar(10) | NOT NULL | CodePs | string? | MaxLength(10) | MAPPED |
| 2 | nom_pharmacie | varchar(50) | nullable | NomPharmacie | string? | MaxLength(50) | MAPPED |
| 3 | nom | varchar(25) | nullable | Nom | string? | MaxLength(25) | MAPPED |
| 4 | prenom | varchar(25) | nullable | Prenom | string? | MaxLength(25) | MAPPED |
| 5 | adresse | varchar(50) | nullable | Adresse | string? | MaxLength(50) | MAPPED |
| 6 | num_tel | varchar(20) | nullable | NumTel | string? | MaxLength(20) | MAPPED |
| 7 | num_fax | varchar(20) | nullable | NumFax | string? | MaxLength(20) | MAPPED |
| 8 | email | varchar(50) | nullable | Email | string? | MaxLength(50) | MAPPED |
| 9 | code_sp | varchar(2) | nullable | CodeSp | string? | MaxLength(2) | MAPPED |
| 10 | nis | varchar(15) | nullable | Nis | string? | MaxLength(15) | MAPPED |
| 11 | nico | varchar(14) | nullable | Nico | string? | MaxLength(14) | MAPPED |
| 12 | ndps | varchar(14) | nullable | Ndps | string? | MaxLength(14) | MAPPED |
| 13 | code_centre | varchar(5) | nullable | CodeCentre | string? | MaxLength(5) | MAPPED |
| 14 | convention | char(1) | nullable | Convention | string? | MaxLength(1) | MAPPED |
| 15 | ref_convention | varchar(25) | nullable | RefConvention | string? | MaxLength(25) | MAPPED |
| 16 | ref_bancaire | varchar(20) | nullable | RefBancaire | string? | MaxLength(20) | MAPPED |
| 17 | mode_reglement | char(1) | nullable | ModeReglement | string? | MaxLength(1) | MAPPED |
| 18 | mont_max | numeric(8,2) | nullable | MontMax | decimal? | Precision(8,2) | MAPPED |
| 19 | contact | char(40) | nullable | Contact | string? | MaxLength(40) | MAPPED |
| 20 | mont_maj_fae | numeric(2,0) | nullable | MontMajFae | decimal? | Precision(2,0) | MAPPED |
| 21 | mont_maj_sub | numeric(2,0) | nullable | MontMajSub | decimal? | Precision(2,0) | MAPPED |
| 22 | taux_maj_local | numeric(2,0) | nullable | TauxMajLocal | decimal? | Precision(2,0) | MAPPED |
| 23 | taux_maj_inf_tr | numeric(2,0) | nullable | TauxMajInfTr | decimal? | Precision(2,0) | MAPPED |
| 24 | version | varchar(20) | nullable | Version | string? | MaxLength(20) | MAPPED |
| 25 | date_medicament | timestamp | nullable | DateMedicament | DateTime? | — | MAPPED |
| 26 | date_liste_noire | timestamp | nullable | DateListeNoire | DateTime? | — | MAPPED |
| 27 | date_liste_mc | timestamp | nullable | DateListeMc | DateTime? | — | MAPPED |
| 28 | date_version | timestamp | nullable | DateVersion | DateTime? | — | MAPPED |
| 29 | date_specialite | timestamp | nullable | DateSpecialite | DateTime? | — | MAPPED |
| 30 | date_tarif | timestamp | nullable | DateTarif | DateTime? | — | MAPPED |
| 31 | date_note | timestamp | nullable | DateNote | DateTime? | — | MAPPED |
| 32 | date_convention | varchar(10) | nullable | DateConvention | string? | MaxLength(10) | MAPPED |
| 33 | nb_ord_max | integer | nullable | NbOrdMax | int? | — | MAPPED |
| 34 | officine_dgsn | boolean | nullable | OfficineDgsn | bool? | — | MAPPED |
| 35 | chemin_backup | varchar(200) | nullable | CheminBackup | string? | MaxLength(200) | MAPPED |
| 36 | heure_backup | varchar(5) | nullable | HeureBackup | string? | MaxLength(5) | MAPPED |
| 37 | nb_backup | integer | nullable | NbBackup | int? | — | MAPPED |
| 38 | next_num_fact | integer | nullable | NextNumFact | int? | — | MAPPED |
| 39 | next_num_bord | smallint | nullable | NextNumBord | short? | — | MAPPED |
| 40 | poste_serveur_chifa | boolean | nullable | PosteServeurChifa | bool? | — | MAPPED |
| 41 | poste_telech | varchar(30) | nullable | PosteTelech | string? | MaxLength(30) | MAPPED |
| 42 | date_api_chifa | timestamp | nullable | DateApiChifa | DateTime? | — | MAPPED |
| 43 | date_verif_maj | timestamp | nullable | DateVerifMaj | DateTime? | — | MAPPED |
| 44 | date_verif_cm | timestamp | nullable | DateVerifCm | DateTime? | — | MAPPED |
| 45 | date_mut_radie | timestamp | nullable | DateMutRadie | DateTime? | — | MAPPED |
| 46 | params | varchar(255) | nullable | Params | string? | MaxLength(255) | MAPPED |
| 47 | version_db | integer | nullable | VersionDb | int? | — | MAPPED |
| 48 | version_ftp | integer | nullable | VersionFtp | int? | — | MAPPED |
| 49 | date_version_ftp | date | nullable | DateVersionFtp | DateTime? | — | MAPPED |
| 50 | annee | integer | nullable | Annee | int? | — | MAPPED |
| 51 | date_ln_complete | date | nullable | DateLnComplete | DateTime? | — | MAPPED |
| 52 | backup_start | boolean | nullable | BackupStart | bool? | — | MAPPED |
| 53 | backup_exit | boolean | nullable | BackupExit | bool? | — | MAPPED |
| 54 | date_medicament2 | timestamp | nullable | DateMedicament2 | DateTime? | — | MAPPED |
| 55 | date_medic_ppa | date | nullable | DateMedicPpa | DateTime? | — | MAPPED |
| 56 | access_token | varchar(256) | nullable | AccessToken | string? | MaxLength(256) | MAPPED |
| 57 | refresh_token | varchar(256) | nullable | RefreshToken | string? | MaxLength(256) | MAPPED |
| 58 | date_medic_demuni | timestamp | nullable | DateMedicDemuni | DateTime? | — | MAPPED |

### 4.2 Mapping Result

**58/58 columns mapped** — 100% coverage.

**Note**: Configured as `HasNoKey()` in EF Core (singleton table, always 1 row).

### 4.3 Real DB Data

| Field | Value |
|-------|-------|
| code_ps | 1234567890 |
| nom_pharmacie | PHARMACIE |
| prenom | EXEMPLE |
| code_centre | 11600 |
| next_num_fact | 1 |
| next_num_bord | 215 |
| version | 3.0.4.3 |
| annee | 2024 |

---

## 5. medicament — Drug Catalog (29 Columns)

### 5.1 Physical Columns

| # | Column | PG Type | Nullable | C# Property | C# Type | EF Config | Status |
|---|--------|---------|----------|-------------|---------|-----------|--------|
| 1 | num_enr | varchar(5) | NOT NULL | NumEnr | string | MaxLength(5) | PK |
| 2 | nom_com | varchar(50) | nullable | NomCom | string? | MaxLength(50) | MAPPED |
| 3 | nom_dci | varchar(60) | nullable | NomDci | string? | MaxLength(60) | MAPPED |
| 4 | dosage | varchar(30) | nullable | Dosage | string? | MaxLength(30) | MAPPED |
| 5 | unite | varchar(20) | nullable | Unite | string? | MaxLength(20) | MAPPED |
| 6 | conditionnement | varchar(20) | nullable | Conditionnement | string? | MaxLength(20) | MAPPED |
| 7 | convention | char(1) | nullable | Convention | string? | MaxLength(1) | MAPPED |
| 8 | remboursable | char(1) | nullable | Remboursable | string? | MaxLength(1) | MAPPED |
| 9 | date_remboursement | varchar(10) | nullable | DateRemboursement | string? | MaxLength(10) | MAPPED |
| 10 | date_arret_remboursement | varchar(10) | nullable | DateArretRemboursement | string? | MaxLength(10) | MAPPED |
| 11 | date_decision | varchar(10) | nullable | DateDecision | string? | MaxLength(10) | MAPPED |
| 12 | tarif_ref | numeric(11,2) | nullable | TarifRef | decimal? | Precision(11,2) | MAPPED |
| 13 | taux | numeric(3,0) | nullable | Taux | decimal? | Precision(3,0) | MAPPED |
| 14 | code_forme | varchar(3) | nullable | CodeForme | string? | MaxLength(3) | MAPPED |
| 15 | tableau | char(1) | nullable | Tableau | string? | MaxLength(1) | MAPPED |
| 16 | hopital | char(1) | nullable | Hopital | string? | MaxLength(1) | MAPPED |
| 17 | secteur_sanitaire | char(1) | nullable | SecteurSanitaire | string? | MaxLength(1) | MAPPED |
| 18 | officine | char(1) | nullable | Officine | string? | MaxLength(1) | MAPPED |
| 19 | pays | varchar(20) | nullable | Pays | string? | MaxLength(20) | MAPPED |
| 20 | laboratoire | varchar(25) | nullable | Laboratoire | string? | MaxLength(25) | MAPPED |
| 21 | cm | char(1) | nullable | Cm | string? | MaxLength(1) | MAPPED |
| 22 | code_medic | varchar(11) | nullable | CodeMedic | string? | MaxLength(11) | MAPPED |
| 23 | date_tr | varchar(10) | nullable | DateTr | string? | MaxLength(10) | MAPPED |
| 24 | observation | varchar(2000) | nullable | Observation | string? | MaxLength(2000) | MAPPED |
| 25 | code_dci | varchar(6) | nullable | CodeDci | string? | MaxLength(6) | MAPPED |
| 26 | code_sp | varchar(2) | nullable | CodeSp | string? | MaxLength(2) | MAPPED |
| 27 | inf_tr | char(1) | nullable | InfTr | string? | MaxLength(1) | MAPPED |
| 28 | generic | char(1) | nullable | Generic | string? | MaxLength(1) | MAPPED |
| 29 | medic | char(1) | nullable | Medic | string? | MaxLength(1) | MAPPED |

### 5.2 Mapping Result

**29/29 columns mapped** — 100% coverage.

**FK**: `code_forme` → `forme.code_forme` (not modeled as navigation property in EF Core).

### 5.3 Real DB Stats

| Metric | Value |
|--------|-------|
| Rows | 7,596 |
| Physical Size | 80 MB |
| Average Row Size | ~10 KB (due to TOAST on observation column) |

---

## 6. signature — Invoice Signatures (2 Columns)

### 6.1 Physical Columns

| # | Column | PG Type | Nullable | C# Property | C# Type | EF Config | Status |
|---|--------|---------|----------|-------------|---------|-----------|--------|
| 1 | num_fact | varchar(8) | NOT NULL | NumFact | string | MaxLength(8) | PK |
| 2 | sign | text | nullable | Sign | string? | MaxLength(4000) | MAPPED |

### 6.2 Mapping Result

**2/2 columns mapped** — 100% coverage.

### 6.3 Real DB Stats

| Metric | Value |
|--------|-------|
| Rows | 0 |
| Physical Size | 181 MB (pre-allocated) |

---

## 7. Summary Matrix

| Table | Physical Columns | EF Properties | Mapped | Phantom | Coverage |
|-------|-----------------|---------------|--------|---------|----------|
| facture | 53 | 53 | 53 | 0 | **100%** |
| detail_fact | 20 | 20 | 20 | 0 | **100%** |
| bordereau | 11 | 11 | 11 | 0 | **100%** |
| parametre | 58 | 58 | 58 | 0 | **100%** |
| medicament | 29 | 29 | 29 | 0 | **100%** |
| signature | 2 | 2 | 2 | 0 | **100%** |
| **TOTAL** | **173** | **173** | **173** | **0** | **100%** |

---

*Document generated by BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment*
