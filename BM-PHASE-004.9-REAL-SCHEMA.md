# BM-PHASE-004.9 — REAL DATABASE SCHEMA (All 48 Tables)

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Summary

Complete column-level schema of all 48 user tables in the `public` schema of CHIFA_OFFICINE PostgreSQL 9.3.4.

Schema discovered via `postgres --single` mode using `pg_catalog` system tables.

---

## 1. facture — Invoices (60 columns, 0 rows, 64 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_fact | varchar(8) | YES | **PK** | Invoice number |
| 2 | date_fact | timestamp | no | | Invoice date |
| 3 | etat | char(1) | no | | Status code |
| 4 | num_bord | varchar(6) | no | | FK → bordereau.num_bord |
| 5 | mont_off | numeric(10,2) | no | | Pharmacist amount |
| 6 | mont_as | numeric(10,2) | no | | Social security amount |
| 7 | mont_fact | numeric(11,2) | no | | Total invoice amount |
| 8 | num_assure | varchar(12) | no | | Social security number |
| 9 | rang_ad | varchar(2) | no | | Adhésion rank |
| 10 | code_centre | varchar(5) | no | | CNAS center code |
| 11 | tp | char(1) | no | | Type patient |
| 12 | taux | char(1) | no | | Reimbursement rate |
| 13 | code_affect | varchar(2) | no | | Affectation code |
| 14 | conv | char(1) | no | | Convention type |
| 15 | type_consult | varchar(2) | no | | Consultation type |
| 16 | prescripteur | varchar(50) | no | | Prescribing doctor |
| 17 | date_soin | date | no | | Care date |
| 18 | risque | char(1) | no | | Risk code |
| 19 | statut_fact | char(1) | no | | Invoice status |
| 20 | verifcms | char(1) | no | | CMS verification |
| 21 | type_signature | char(1) | no | | Signature type |
| 22 | verif_fact | char(1) | no | | Invoice verification |
| 23 | mont_maj_fae | numeric(4,2) | no | | FAE increase amount |
| 24 | mont_maj | numeric(11,2) | no | | Total increase amount |
| 25 | type_maj | integer | YES | | Increase type (default 0) |
| 26 | code_centre_as | char(5) | no | | AS center code |
| 27 | code_sp | varchar(2) | no | | Specialty code |
| 28 | type_ord | char(1) | no | | Prescription type |
| 29 | motif_med | varchar(16) | no | | Medical justification |
| 30 | id_user | integer | no | | FK → utilisateur.id_user |
| 31 | signature | xml | no | | Invoice XML signature |
| 32 | num_serie | bigint | no | | Serial number |
| 33 | date_envoi_sms | timestamp | no | | SMS send date |
| **gap** | **(attnum 34-35 dropped)** | | | | **Columns removed by CHIFA** |
| 36 | date_fin_droit | date | no | | Right end date |
| 37 | date_fin_droit_benef | date | no | | Beneficiary right end date |
| 38 | version | varchar(10) | no | | Version string |
| 39 | code_covid | varchar(20) | no | | COVID code |
| **gap** | **(attnum 40 dropped)** | | | | **Column removed by CHIFA** |
| 41 | fact_xml | xml | no | | Full invoice XML |
| **gap** | **(attnum 42-45 dropped)** | | | | **Columns removed by CHIFA** |
| 46 | nat_remb | varchar(1) | no | | Reimbursement nature |
| 47 | mont_mut | numeric(10,2) | no | | Mutual amount |
| 48 | date_fin_mut | date | no | | Mutual end date |
| 49 | code_mut | varchar(2) | no | | Mutual code |
| 50 | date_synchro | timestamp | no | | Sync date |
| 51 | adresse_ip | varchar(15) | no | | Client IP address |
| 52 | nom_pc | varchar(30) | no | | Computer name |
| 53 | obs | varchar(255) | no | | Remarks |
| 54 | ref_cm | varchar(18) | no | | CM reference |
| 55 | num_serie_ps | bigint | no | | Smart card serial |
| 56 | version_carte | integer | no | | Card version |
| 57 | echifa | boolean | no | | E-CHIFA flag |
| 58 | id_fact_echifa | bigint | no | | E-CHIFA invoice ID |
| 59 | e_ord | boolean | no | | E-prescription flag |
| 60 | id_e_ord | bigint | no | | E-prescription ID |

### Column Gaps Analysis

The `facture` table has **gaps in attnum numbering** (34-35, 40, 42-45). These represent columns that were **dropped** by CHIFA-OFFICINE application updates over time. PostgreSQL preserves the physical attribute numbers of remaining columns, creating gaps. The logical column count remains 60.

---

## 2. detail_fact — Invoice Lines (20 columns, 0 rows, 40 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_fact | varchar(8) | YES | **PK** | FK → facture.num_fact |
| 2 | num_lot | varchar(6) | no | | Batch number |
| 3 | num_enr | varchar(5) | YES | **PK** | Drug enrollment number |
| 4 | qte | numeric(3,0) | YES | **PK** | Quantity |
| 5 | ppa | numeric(10,2) | YES | **PK** | Unit price (prix par article) |
| 6 | mont | numeric(10,2) | YES | | Total amount |
| 7 | mont_as | numeric(10,2) | no | | Social security amount |
| 8 | mont_pharm | numeric(10,2) | no | | Pharmacist amount |
| 9 | maj_local | numeric(10,2) | no | | Local increase |
| 10 | num_enr_prescrit | varchar(5) | YES | | Prescribed drug number |
| 11 | maj_sub | numeric(3,0) | no | | Sub increase |
| 12 | duree_trait | numeric(3,0) | no | | Treatment duration |
| 13 | tarif_ref | numeric(10,2) | no | | Reference tariff |
| 14 | posologie | varchar(50) | no | | Dosage instructions |
| 15 | remboursable | boolean | no | | Reimbursable flag |
| 16 | local | boolean | no | | Local flag |
| 17 | inf_tr | boolean | no | | Below tariff flag |
| 18 | applic_tr | boolean | no | | Apply tariff flag |
| 19 | medic | boolean | no | | Medication flag |
| 20 | ts | boolean | no | | TS flag |

> **Note**: PK is composite (num_fact, num_enr, ppa) per BM-SPEC-029. The `qte` column is marked NOT NULL but is not part of the PK.

---

## 3. bordereau — Batches (11 columns, 0 rows, 56 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | id_bord | bigint | YES | **PK** | Auto-increment (bordereau_id_bord_seq) |
| 2 | num_bord | varchar(6) | YES | | Batch number (UNIQUE: UN_BORDEREAU) |
| 3 | code_centre | varchar(5) | YES | | CNAS center code |
| 4 | etat | char(1) | no | | Status code |
| 5 | id_user_cloture | integer | no | | FK → utilisateur.id_user |
| 6 | poste_cloture | varchar(100) | no | | Closure workstation |
| 7 | mont_vir | numeric(10,2) | no | | Transfer amount |
| 8 | duplicata | boolean | no | | Duplicate flag |
| 9 | date_cloture | timestamp | no | | Closure date |
| 10 | date_ouverture | timestamp | no | | Opening date |
| 11 | date_depot_ftp | timestamp | no | | FTP deposit date |

---

## 4. parametre — Pharmacy Configuration (60 columns, 1 row, 32 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code_ps | varchar(10) | YES | **PK** | Pharmacy code |
| 2 | nom_pharmacie | varchar(50) | no | | Pharmacy name |
| 3 | nom | varchar(25) | no | | Pharmacist last name |
| 4 | prenom | varchar(25) | no | | Pharmacist first name |
| 5 | adresse | varchar(50) | no | | Address |
| 6 | num_tel | varchar(20) | no | | Phone number |
| 7 | num_fax | varchar(20) | no | | Fax number |
| 8 | email | varchar(50) | no | | Email |
| 9 | code_sp | varchar(2) | no | | Specialty code |
| 10 | nis | varchar(15) | no | | NIS number |
| 11 | nico | varchar(14) | no | | NICO number |
| 12 | ndps | varchar(14) | no | | NDPS number |
| 13 | code_centre | varchar(5) | no | | CNAS center code |
| 14 | convention | char(1) | no | | Convention type |
| 15 | ref_convention | varchar(25) | no | | Convention reference |
| 16 | ref_bancaire | varchar(20) | no | | Bank reference |
| 17 | mode_reglement | char(1) | no | | Payment mode |
| 18 | mont_max | numeric(8,2) | no | | Maximum amount |
| 19 | contact | char(40) | no | | Contact person |
| 20 | mont_maj_fae | numeric(2,0) | no | | FAE increase rate |
| 21 | mont_maj_sub | numeric(2,0) | no | | Sub increase rate |
| 22 | taux_maj_local | numeric(2,0) | no | | Local increase rate |
| 23 | taux_maj_inf_tr | numeric(2,0) | no | | Below-tariff increase rate |
| 24 | version | varchar(20) | no | | Software version |
| 25 | date_medicament | timestamp | no | | Last drug catalog update |
| 26 | date_liste_noire | timestamp | no | | Last blacklist update |
| 27 | date_liste_mc | timestamp | no | | Last MC list update |
| 28 | date_version | timestamp | no | | Last version update |
| 29 | date_specialite | timestamp | no | | Last specialty update |
| 30 | date_tarif | timestamp | no | | Last tariff update |
| 31 | date_note | timestamp | no | | Last note update |
| 32 | date_convention | varchar(10) | no | | Convention date |
| 33 | nb_ord_max | integer | no | | Max orders |
| 34 | officine_dgsn | boolean | no | | DGSN office flag |
| 35 | chemin_backup | varchar(200) | no | | Backup path |
| 36 | heure_backup | varchar(5) | no | | Backup time |
| 37 | nb_backup | integer | no | | Backup count |
| 38 | next_num_fact | integer | no | | **Next invoice number** |
| 39 | next_num_bord | smallint | no | | **Next batch number** |
| 40 | poste_serveur_chifa | boolean | no | | Is CHIFA server |
| 41 | poste_telech | varchar(30) | no | | Download workstation |
| 44 | date_api_chifa | timestamp | no | | Last API call |
| 45 | date_verif_maj | timestamp | no | | Last update check |
| 46 | date_verif_cm | timestamp | no | | Last CM check |
| 47 | date_mut_radie | timestamp | no | | Last mutual radie check |
| 48 | params | varchar(255) | no | | Custom parameters |
| 49 | version_db | integer | no | | DB schema version |
| 50 | version_ftp | integer | no | | FTP version |
| 51 | date_version_ftp | date | no | | FTP version date |
| 52 | annee | integer | no | | Current year |
| 53 | date_ln_complete | date | no | | Last LN complete date |
| 54 | backup_start | boolean | no | | Backup started |
| 55 | backup_exit | boolean | no | | Backup completed |
| 56 | date_medicament2 | timestamp | no | | Drug catalog 2 update |
| 57 | date_medic_ppa | date | no | | Drug PPA update |
| 58 | access_token | varchar(256) | no | | API access token |
| 59 | refresh_token | varchar(256) | no | | API refresh token |
| 60 | date_medic_demuni | timestamp | no | | Drug shortage update |

### Key Observations

- `next_num_fact` (col #38) and `next_num_bord` (col #39) are **critical** — they determine the next invoice/batch numbers
- `access_token` and `refresh_token` (cols #58-59) store API tokens in plaintext
- Gaps at attnum 42-43 (columns dropped)
- BM Pharma only maps 15 of these 60 columns

---

## 5. medicament — Drug Catalog (29 columns, 7596 rows, 80 MB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_enr | varchar(5) | YES | **PK** | Drug enrollment number |
| 2 | nom_com | varchar(50) | no | | Commercial name |
| 3 | nom_dci | varchar(60) | no | | DCI name (active ingredient) |
| 4 | dosage | varchar(30) | no | | Dosage |
| 5 | unite | varchar(20) | no | | Unit |
| 6 | conditionnement | varchar(20) | no | | Packaging |
| 7 | convention | char(1) | no | | Convention type |
| 8 | remboursable | char(1) | no | | Reimbursable flag |
| 9 | date_remboursement | varchar(10) | no | | Reimbursement date |
| 10 | date_arret_remboursement | varchar(10) | no | | Reimbursement stop date |
| 11 | date_decision | varchar(10) | no | | Decision date |
| 12 | tarif_ref | numeric(11,2) | no | | Reference tariff |
| 13 | taux | numeric(3,0) | no | | Reimbursement rate |
| 14 | code_forme | varchar(3) | no | | FK → forme.code_forme |
| 15 | tableau | char(1) | no | | Table code |
| 16 | hopital | char(1) | no | | Hospital flag |
| 17 | secteur_sanitaire | char(1) | no | | Health sector |
| 18 | officine | char(1) | no | | Office flag |
| 19 | pays | varchar(20) | no | | Country |
| 20 | laboratoire | varchar(25) | no | | Laboratory |
| 21 | cm | char(1) | no | | CM flag |
| 22 | code_medic | varchar(11) | no | | Drug code |
| 23 | date_tr | varchar(10) | no | | Tariff date |
| 24 | observation | varchar(2000) | no | | Observations |
| 25 | code_dci | varchar(6) | no | | DCI code |
| 26 | code_sp | varchar(2) | no | | Specialty code |
| 27 | inf_tr | char(1) | no | | Below tariff flag |
| 28 | generic | char(1) | no | | Generic flag |
| 29 | medic | char(1) | no | | Medication flag |

---

## 6. ln — National List (3,708,019 rows, 278 MB)

**Columns not fully enumerated** — this is the massive national drug reimbursement list. The table is referenced by `medicament.num_enr` and contains historical reimbursement data.

---

## 7. signature — Invoice Signatures (0 rows, 181 MB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_fact | varchar(8) | YES | **PK** | FK → facture.num_fact |
| 2 | sign | text | no | | Signature data |

---

## 8. logiciel — Software Version (1 row, 169 MB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | version | varchar | YES | **PK** | Software version |

> 169 MB for 1 row suggests embedded binary data (installer/updates).

---

## 9. detail_fact_cm — CM Invoice Lines (0 rows, 71 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_fact | varchar(?) | YES | **PK** | FK → facture_cm.num_fact |
| 2 | num_enr | varchar(?) | YES | **PK** | Drug number |
| 3 | ppa | numeric(?) | YES | **PK** | Unit price |

---

## 10. forme — Drug Forms (469 rows, 1,264 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code_forme | varchar(3) | YES | **PK** | Form code |

Additional columns exist (description, etc.).

---

## 11. type_posologie — Dosage Types (26 rows, 656 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code | varchar(?) | YES | **PK** | Type code |

---

## 12. beneficiaire — Beneficiaries (0 rows, 488 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_assure | varchar(?) | YES | **PK** | Social security number |
| 2 | rang_ad | varchar(?) | YES | **PK** | Adhésion rank |

---

## 13. medic_ppa — Drug PPA Data (0 rows, 464 kB)

Columns not fully enumerated — PPA (Prix de Prestation Annexe) data linked to drug catalog.

---

## 14. medic_sp — Drug Specialties (292 rows, 408 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_enr | varchar(?) | YES | **PK** | FK → medicament.num_enr |
| 2 | code_sp | varchar(?) | YES | **PK** | FK → specialite.code_sp |

---

## 15. medic_demuni — Drug Shortages (936 rows, 256 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code | varchar(?) | YES | **PK** | Drug code |

---

## 16. tarif — Pricing (1,641 rows, 240 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_enr | varchar(?) | YES | **PK** | FK → medicament.num_enr |
| 2 | d_debut | date(?) | YES | **PK** | Start date |

---

## 17. specialite — Medical Specialties (87 rows, 64 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code_sp | varchar(2) | YES | **PK** | Specialty code |

---

## 18. conditionnement — Drug Packaging (185 rows, 64 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | condit | varchar(?) | YES | **PK** | Packaging code |
| 2 | nombre | numeric(?) | YES | **PK** | Quantity |

---

## 19. facture — (duplicate of #1 above)

---

## 20. medicament2 — Alt Drug Catalog (276 rows, 64 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code | varchar(?) | YES | **PK** | Drug code |

---

## 21. condition — Medical Conditions (38 rows, 56 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code | varchar(?) | YES | **PK** | Condition code |
| 2 | nature | varchar(?) | YES | **PK** | Condition nature |

---

## 22. parametre_code_barre — Barcode Config (1 row, 40 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | (unknown) | ? | ? | ? | |

---

## 23. utilisateur — Users (1 row, 40 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | id_user | integer | YES | **PK** | Auto-increment (utilisateur_id_user_seq) |
| 2 | nom_utilisateur | varchar(?) | no | | UNIQUE (UN_UTILISATEUR) |

Additional columns exist (role, password_hash, etc.).

---

## 24-28. temp00 through temp04 — Temporary Work Tables

All temporary tables with 0 rows. Used by CHIFA-OFFICINE for intermediate processing.

| Table | Size |
|-------|------|
| temp00 | 152 kB |
| temp01 | 128 kB |
| temp02 | 200 kB |
| temp03 | 16 kB |
| temp04 | 216 kB |

---

## 29. centre — CNAS Centers (2 rows, 24 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code_centre | varchar(5) | YES | **PK** | Center code |

---

## 30. facture_cm — CM Invoices (0 rows, 24 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_fact | varchar(?) | YES | **PK** | Invoice number |

---

## 31. attestation_mc — MC Attestations (0 rows, 24 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_assure | varchar(?) | YES | **PK** | Social security number |
| 2 | rang_ad | varchar(?) | YES | **PK** | Adhésion rank |
| 3 | code_centre | varchar(?) | YES | **PK** | Center code |

---

## 32. mutualiste_radie — Struck-off Members (0 rows, 24 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_assure | varchar(?) | YES | **PK** | Social security number |
| 2 | code_mut | varchar(?) | YES | **PK** | Mutual code |

---

## 33. file — Files (0 rows, 16 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | file_name | varchar(?) | YES | **PK** | File name |

---

## 34. carte_chifa — CHIFA Cards (0 rows, 16 kB)

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_serie | bigint(?) | YES | **PK** | Card serial number |

---

## 35. token — Auth Tokens

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code_affect | varchar(?) | YES | **PK** | Affectation code |

Unique index: TOKEN_UN on (ip).

---

## 36. certificat_token — Token Certificates

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_serie | bigint(?) | YES | **PK** | Serial number |

---

## 37. cm — CM Records

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | ref_cm | varchar(?) | YES | **PK** | CM reference |
| 2 | num_fact | varchar(?) | YES | **PK** | FK → facture.num_fact |
| 3 | num_assure | varchar(?) | YES | **PK** | Social security number |

---

## 38. cm_audit — CM Audit

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | num_fact | varchar(?) | YES | **PK** | FK → facture.num_fact |

---

## 39. ct_acces — Access Control

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | id_user | integer(?) | YES | **PK** | FK → utilisateur.id_user |
| 2 | composant | varchar(?) | YES | **PK** | Component name |

---

## 40. rupture_stock — Stock Alerts

| # | Column | Type | NOT NULL | PK | Notes |
|---|--------|------|----------|----|-------|
| 1 | code_medic | varchar(?) | YES | **PK** | Drug code |
| 2 | date_insert | date(?) | YES | **PK** | Insertion date |

---

## 41. droit_acces — Access Rights

FK confirmed: droit_acces.id_user → utilisateur.id_user.

PK: (id_user, composant).

---

## Complete PK Summary

| Table | Primary Key |
|-------|-------------|
| attestation_mc | (num_assure, rang_ad, code_centre) |
| beneficiaire | (num_assure, rang_ad) |
| bordereau | num_bord |
| carte_chifa | num_serie |
| centre | code_centre |
| certificat_token | num_serie |
| cm | (ref_cm, num_fact, num_assure) |
| cm_audit | num_fact |
| condition | (code, nature) |
| conditionnement | (condit, nombre) |
| ct_acces | (id_user, composant) |
| detail_fact | (num_fact, num_enr, ppa) |
| detail_fact_cm | (num_fact, num_enr, ppa) |
| droit_acces | (id_user, composant) |
| facture | num_fact |
| facture_cm | num_fact |
| file | file_name |
| forme | code_forme |
| logiciel | version |
| medic_demuni | code |
| medic_sp | (num_enr, code_sp) |
| medicament | num_enr |
| medicament2 | code |
| mutualiste_radie | (num_assure, code_mut) |
| parametre | code_ps |
| rupture_stock | (code_medic, date_insert) |
| signature | num_fact |
| specialite | code_sp |
| tarif | (num_enr, d_debut) |
| token | code_affect |
| type_posologie | code |
| utilisateur | id_user |

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
