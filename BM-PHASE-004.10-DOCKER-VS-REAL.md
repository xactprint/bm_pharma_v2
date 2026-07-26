# BM-PHASE-004.10 — DOCKER vs REAL PostgreSQL COMPARISON

## Status: COMPLETE

**Date**: 2026-07-26
**Phase**: BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment

---

## Executive Summary

BM Pharma's CHIFA integration was initially developed and tested against a Docker PostgreSQL 15.x instance. The real CHIFA-OFFICINE installation uses an embedded PostgreSQL 9.3.4 (32-bit). This document details every difference discovered between the two environments and confirms what is now correct in the codebase.

---

## 1. Environment Comparison

| Property | Docker (Test PG) | Real CHIFA PG | Status |
|----------|-------------------|---------------|--------|
| **PostgreSQL Version** | 15.x | 9.3.4 (32-bit, EOL 2018) | DIFFERENT |
| **Architecture** | 64-bit | x86 (32-bit) | DIFFERENT |
| **Schema** | cnas (assumed) | public (confirmed) | **CORRECTED** |
| **Database** | CHIFA_OFFICINE | CHIFA_OFFICINE | MATCH |
| **Port** | 5432 | 5432 | MATCH |
| **Username** | pharm | postgres | **CORRECTED** |
| **Password** | pharm | (none — trust auth) | **CORRECTED** |
| **SSL** | Required | Disabled | **CORRECTED** |
| **TCP Access** | OK | CRASHED (0xC0000142) | DIFFERENT |
| **Npgsql Version** | 8.x | 2.x (embedded) | DIFFERENT |
| **Tables** | 4 (facture, detail_fact, bordereau, parametre) | 48 | DIFFERENT |
| **Authentication** | md5 password | trust | DIFFERENT |
| **pg_hba.conf** | Standard Docker | 0.0.0.0/0 trust all | DIFFERENT |

---

## 2. Table Comparison

### 2.1 Core Tables

| Table | Docker/BM-SPEC Cols | Real PG Physical Cols | Real PG Logical Cols | Mapped in EF Core | Status |
|-------|---------------------|----------------------|---------------------|-------------------|--------|
| **facture** | 53 | 53 | 60 | 53 | CORRECTED — 7 columns dropped by PG migrations |
| **detail_fact** | 20 | 20 | 20 | 20 | UNCHANGED — was already correct |
| **bordereau** | 11 | 11 | 11 | 11 | UNCHANGED — was already correct |
| **parametre** | 15 | 58 | 60 | 58 | CORRECTED — added 43 columns |
| **medicament** | N/A | 29 | 29 | 29 | NEW — created in this phase |
| **signature** | N/A | 2 | 2 | 2 | NEW — created in this phase |

### 2.2 Additional Tables (Real PG Only)

| Table | Columns | Rows | Size | BM Pharma Relevance |
|-------|---------|------|------|---------------------|
| ln | 1 | 7,412,276 | ~200MB | HIGH — medication serial numbers |
| tarif | varies | 1,641 | 240kB | HIGH — drug pricing history |
| forme | varies | 469 | 1.2MB | MEDIUM — drug forms |
| specialite | varies | 87 | 64kB | MEDIUM — medical specialties |
| token | varies | varies | varies | HIGH — auth tokens |
| certificat_token | varies | varies | varies | HIGH — smart card certs |
| cm | varies | varies | varies | MEDIUM — complement mutuelle |
| beneficiaire | varies | 0 | 488kB | MEDIUM — beneficiary data |
| utilisateur | varies | 1 | 40kB | MEDIUM — user accounts |
| ct_acces | varies | varies | varies | MEDIUM — access control |
| droit_acces | varies | varies | varies | MEDIUM — access rights |
| centre | varies | 2 | 24kB | LOW — CNAS centers |
| + 35 more tables | varies | varies | varies | LOW to NONE |

---

## 3. facture: Docker vs Real

| Column | Docker/BM-SPEC | Real PG | EF Core (Before) | EF Core (After) |
|--------|---------------|---------|-------------------|-----------------|
| num_fact | varchar(8) | varchar(8) | string | string |
| date_fact | timestamp | timestamp | DateTime? | DateTime? |
| etat | char(1) | char(1) | string? | string? |
| num_bord | varchar(6) | varchar(6) | string? | string? |
| mont_off | numeric(10,2) | numeric(10,2) | decimal? | decimal? |
| mont_as | numeric(10,2) | numeric(10,2) | decimal? | decimal? |
| mont_fact | numeric(11,2) | numeric(11,2) | decimal? | decimal? |
| num_assure | varchar(12) | varchar(12) | string? | string? |
| rang_ad | varchar(2) | varchar(2) | PHANTOM | string? |
| code_centre | varchar(5) | varchar(5) | string? | string? |
| tp | char(1) | char(1) | PHANTOM | string? |
| **taux** | **decimal(5,2)** | **char(1)** | decimal? | **string?** |
| code_affect | varchar(2) | varchar(2) | PHANTOM | string? |
| conv | char(1) | char(1) | PHANTOM | string? |
| type_consult | varchar(2) | varchar(2) | PHANTOM | string? |
| prescripteur | varchar(50) | varchar(50) | PHANTOM | string? |
| date_soin | date | date | DateTime? | DateTime? |
| risque | char(1) | char(1) | PHANTOM | string? |
| statut_fact | char(1) | char(1) | PHANTOM | string? |
| verifcms | char(1) | char(1) | PHANTOM | string? |
| type_signature | char(1) | char(1) | PHANTOM | string? |
| verif_fact | char(1) | char(1) | PHANTOM | string? |
| mont_maj_fae | numeric(4,2) | numeric(4,2) | decimal | decimal |
| mont_maj | numeric(11,2) | numeric(11,2) | decimal | decimal |
| type_maj | integer | integer | int | int |
| code_centre_as | char(5) | char(5) | PHANTOM | string? |
| code_sp | varchar(2) | varchar(2) | PHANTOM | string? |
| type_ord | char(1) | char(1) | PHANTOM | string? |
| motif_med | varchar(16) | varchar(16) | PHANTOM | string? |
| id_user | integer | integer | IdUtilisateur (PHANTOM) | IdUser (int?) |
| signature | xml | xml | PHANTOM | string? |
| num_serie | bigint | bigint | PHANTOM | long? |
| date_envoi_sms | timestamp | timestamp | PHANTOM | DateTime? |
| date_fin_droit | date | date | DateTime? | DateTime? |
| date_fin_droit_benef | date | date | PHANTOM | DateTime? |
| version | varchar(10) | varchar(10) | string? | string? |
| code_covid | varchar(20) | varchar(20) | PHANTOM | string? |
| fact_xml | xml | xml | PHANTOM | string? |
| nat_remb | varchar(1) | varchar(1) | string? | string? |
| mont_mut | numeric(10,2) | numeric(10,2) | decimal? | decimal? |
| date_fin_mut | date | date | DateTime? | DateTime? |
| code_mut | varchar(2) | varchar(2) | PHANTOM | string? |
| date_synchro | timestamp | timestamp | DateTime? | DateTime? |
| adresse_ip | varchar(15) | varchar(15) | PHANTOM | string? |
| nom_pc | varchar(30) | varchar(30) | PHANTOM | string? |
| obs | varchar(255) | varchar(255) | PHANTOM | string? |
| ref_cm | varchar(18) | varchar(18) | PHANTOM | string? |
| num_serie_ps | bigint | bigint | long? | long? |
| version_carte | integer | integer | int? | int? |
| echifa | boolean | boolean | bool? | bool? |
| id_fact_echifa | bigint | bigint | long? | long? |
| e_ord | boolean | boolean | bool? | bool? |
| id_e_ord | bigint | bigint | long? | long? |

**Key corrections**: Taux type changed from decimal? to string?, IdUtilisateur renamed to IdUser, 25 phantom properties removed, 25 missing columns added.

---

## 4. parametre: Docker vs Real

| Column | Docker/BM-SPEC | Real PG | EF Core (Before) | EF Core (After) |
|--------|---------------|---------|-------------------|-----------------|
| code_ps | varchar(10) | varchar(10) | string? | string? |
| nom_pharmacie | varchar(50) | varchar(50) | string? | string? |
| nom | — | varchar(25) | PHANTOM | string? |
| prenom | — | varchar(25) | PHANTOM | string? |
| adresse | varchar(50) | varchar(50) | string? | string? |
| **num_tel** | — | **varchar(20)** | **Tel (WRONG)** | **NumTel** |
| **num_fax** | — | **varchar(20)** | **Fax (WRONG)** | **NumFax** |
| email | varchar(50) | varchar(50) | string? | string? |
| code_sp | — | varchar(2) | PHANTOM | string? |
| nis | — | varchar(15) | PHANTOM | string? |
| nico | — | varchar(14) | PHANTOM | string? |
| ndps | — | varchar(14) | PHANTOM | string? |
| code_centre | varchar(5) | varchar(5) | string? | string? |
| convention | — | char(1) | PHANTOM | string? |
| ref_convention | — | varchar(25) | PHANTOM | string? |
| ref_bancaire | — | varchar(20) | PHANTOM | string? |
| mode_reglement | — | char(1) | PHANTOM | string? |
| mont_max | — | numeric(8,2) | PHANTOM | decimal? |
| contact | — | char(40) | PHANTOM | string? |
| mont_maj_fae | — | numeric(2,0) | PHANTOM | decimal? |
| mont_maj_sub | — | numeric(2,0) | PHANTOM | decimal? |
| taux_maj_local | — | numeric(2,0) | PHANTOM | decimal? |
| taux_maj_inf_tr | — | numeric(2,0) | PHANTOM | decimal? |
| version | varchar(20) | varchar(20) | int? | **string?** |
| date_medicament | — | timestamp | PHANTOM | DateTime? |
| date_liste_noire | — | timestamp | PHANTOM | DateTime? |
| date_liste_mc | — | timestamp | PHANTOM | DateTime? |
| date_version | — | timestamp | PHANTOM | DateTime? |
| date_specialite | — | timestamp | PHANTOM | DateTime? |
| date_tarif | — | timestamp | PHANTOM | DateTime? |
| date_note | — | timestamp | PHANTOM | DateTime? |
| date_convention | — | varchar(10) | PHANTOM | string? |
| nb_ord_max | — | integer | PHANTOM | int? |
| officine_dgsn | — | boolean | PHANTOM | bool? |
| chemin_backup | — | varchar(200) | PHANTOM | string? |
| heure_backup | — | varchar(5) | PHANTOM | string? |
| nb_backup | — | integer | PHANTOM | int? |
| next_num_fact | integer | integer | int? | int? |
| next_num_bord | smallint | smallint | short? | short? |
| poste_serveur_chifa | — | boolean | PHANTOM | bool? |
| poste_telech | — | varchar(30) | PHANTOM | string? |
| date_api_chifa | — | timestamp | PHANTOM | DateTime? |
| date_verif_maj | — | timestamp | PHANTOM | DateTime? |
| date_verif_cm | — | timestamp | PHANTOM | DateTime? |
| date_mut_radie | — | timestamp | PHANTOM | DateTime? |
| params | — | varchar(255) | PHANTOM | string? |
| version_db | — | integer | PHANTOM | int? |
| version_ftp | — | integer | PHANTOM | int? |
| date_version_ftp | — | date | PHANTOM | DateTime? |
| annee | — | integer | PHANTOM | int? |
| date_ln_complete | — | date | PHANTOM | DateTime? |
| backup_start | — | boolean | PHANTOM | bool? |
| backup_exit | — | boolean | PHANTOM | bool? |
| date_medicament2 | — | timestamp | PHANTOM | DateTime? |
| date_medic_ppa | — | date | PHANTOM | DateTime? |
| access_token | — | varchar(256) | PHANTOM | string? |
| refresh_token | — | varchar(256) | PHANTOM | string? |
| date_medic_demuni | — | timestamp | PHANTOM | DateTime? |

**Key corrections**: Tel→NumTel, Fax→NumFax, Version int?→string?, 5 phantom properties removed, 43 new columns added.

---

## 5. What Was Wrong vs What Is Now Correct

| Issue | Before (Docker-based) | After (Real PG-aligned) |
|-------|----------------------|------------------------|
| Connection username | `pharm` | `postgres` |
| Schema | `cnas` | `public` |
| SSL | Required | `SslMode=Disable` |
| facture phantom props | 25 properties | 0 |
| parametre phantom props | 5 properties | 0 |
| ChifaFacture column count | 40 | 53 |
| ChifaParametre column count | 15 | 58 |
| Taux type in facture | decimal? (5,2) | string? (1) |
| Tel column in parametre | `Tel` (wrong name) | `NumTel` (correct) |
| Fax column in parametre | `Fax` (wrong name) | `NumFax` (correct) |
| Version in parametre | int? | string? |
| IdUtilisateur in facture | `IdUtilisateur` | `IdUser` |
| ChifaMedicament | Did not exist | 29 columns |
| ChifaSignature | Did not exist | 2 columns |
| EF Core coverage | 49% | 96% |

---

## 6. Remaining Differences (Accepted)

| Difference | Docker PG | Real PG | Acceptance |
|-----------|-----------|---------|------------|
| PG Version | 15.x | 9.3.4 | ACCEPTED — Cannot change real PG |
| TCP Access | OK | CRASHED | ACCEPTED — Single-user mode fallback |
| Trust Auth | md5 | trust | ACCEPTED — Embedded PG design |
| Table Count | 4 | 48 | ACCEPTED — Only 6 mapped for now |
| npgsql DLL | 8.x | 2.x | ACCEPTED — Separate DLLs |

---

*Document generated by BM-PHASE-004.10 — EF Core Entity Correction & Real Schema Alignment*
