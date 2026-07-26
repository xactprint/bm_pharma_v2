# BM-PHASE-004.9 — DATABASE RELATIONSHIPS

## Status: ✅ **COMPLETE — READ-ONLY DISCOVERY**

**Date**: 2026-07-26
**Sub-phase**: BM-PHASE-004.9 — Real CHIFA Environment & Database Discovery

---

## Executive Summary

Complete inventory of primary keys, foreign keys, unique constraints, and indexes discovered in the CHIFA_OFFICINE database. Relationship discovery was limited by single-user mode access, but confirmed relationships provide a clear picture of the data model.

---

## 1. Primary Keys

### 1.1 Single-Column PKs

| Table | PK Column | Type | Sequence |
|-------|-----------|------|----------|
| bordereau | id_bord | bigint | bordereau_id_bord_seq |
| carte_chifa | num_serie | bigint | — |
| centre | code_centre | varchar(5) | — |
| certificat_token | num_serie | bigint | — |
| facture | num_fact | varchar(8) | — |
| facture_cm | num_fact | varchar(8) | — |
| file | file_name | varchar(?) | — |
| forme | code_forme | varchar(3) | — |
| logiciel | version | varchar(?) | — |
| medic_demuni | code | varchar(?) | — |
| medicament | num_enr | varchar(5) | — |
| medicament2 | code | varchar(?) | — |
| parametre | code_ps | varchar(10) | — |
| signature | num_fact | varchar(8) | — |
| specialite | code_sp | varchar(2) | — |
| token | code_affect | varchar(?) | — |
| type_posologie | code | varchar(?) | — |
| utilisateur | id_user | integer | utilisateur_id_user_seq |

### 1.2 Composite PKs

| Table | PK Columns | Types |
|-------|-----------|-------|
| attestation_mc | (num_assure, rang_ad, code_centre) | varchar+varchar+varchar |
| beneficiaire | (num_assure, rang_ad) | varchar+varchar |
| cm | (ref_cm, num_fact, num_assure) | varchar+varchar+varchar |
| condition | (code, nature) | varchar+varchar |
| conditionnement | (condit, nombre) | varchar+numeric |
| ct_acces | (id_user, composant) | integer+varchar |
| detail_fact | (num_fact, num_enr, ppa) | varchar+varchar+numeric |
| detail_fact_cm | (num_fact, num_enr, ppa) | varchar+varchar+numeric |
| droit_acces | (id_user, composant) | integer+varchar |
| medic_sp | (num_enr, code_sp) | varchar+varchar |
| mutualiste_radie | (num_assure, code_mut) | varchar+varchar |
| rupture_stock | (code_medic, date_insert) | varchar+date |
| tarif | (num_enr, d_debut) | varchar+date |

---

## 2. Foreign Keys (Confirmed)

| # | Child Table | Child Column(s) | Parent Table | Parent Column | On Delete | On Update |
|---|-------------|-----------------|--------------|---------------|-----------|-----------|
| 1 | detail_fact | num_fact | facture | num_fact | ? | ? |
| 2 | facture | num_bord | bordereau | num_bord | ? | ? |
| 3 | facture_cm | num_bord | bordereau | num_bord | ? | ? |
| 4 | droit_acces | id_user | utilisateur | id_user | ? | ? |
| 5 | signature | num_fact | facture | num_fact | ? | ? |

### 2.1 Inferred Relationships (Not Explicit FK, But Logical)

| Child Table | Child Column | Parent Table | Parent Column | Evidence |
|-------------|-------------|--------------|---------------|----------|
| detail_fact | num_fact | facture | num_fact | Same name, same type, PK overlap |
| cm | num_fact | facture | num_fact | Same name, same type |
| cm_audit | num_fact | facture | num_fact | Same name, same type |
| facture_cm | num_fact | facture_cm | num_fact | Same name, same type |
| medic_sp | num_enr | medicament | num_enr | Same name, same type |
| tarif | num_enr | medicament | num_enr | Same name, same type |
| medic_ppa | num_enr(?) | medicament | num_enr | Same name, same type |
| facture | code_centre | centre | code_centre | Same name, same type |
| parametre | code_centre | centre | code_centre | Same name, same type |
| facture | code_sp | specialite | code_sp | Same name, same type |
| parametre | code_sp | specialite | code_sp | Same name, same type |
| medicament | code_forme | forme | code_forme | Same name, same type |
| ct_acces | id_user | utilisateur | id_user | Same name, same type |
| cm_audit | num_fact | facture | num_fact | Same name, same type |

---

## 3. Unique Constraints

| # | Index Name | Table | Column(s) | Type |
|---|-----------|-------|-----------|------|
| 1 | facture_pkey | facture | num_fact | PRIMARY KEY |
| 2 | detail_fact_pkey | detail_fact | (num_fact, num_enr, ppa) | PRIMARY KEY |
| 3 | bordereau_pkey | bordereau | id_bord | PRIMARY KEY |
| 4 | UN_BORDEREAU | bordereau | num_bord | UNIQUE |
| 5 | UN_UTILISATEUR | utilisateur | nom_utilisateur | UNIQUE |
| 6 | TOKEN_UN | token | ip | UNIQUE |
| 7 | parametre_pkey | parametre | code_ps | PRIMARY KEY |
| 8 | signature_pkey | signature | num_fact | PRIMARY KEY |
| 9 | All other PKs | various | various | PRIMARY KEY |

---

## 4. Indexes

### 4.1 Primary Key Indexes (B-tree, Unique)

All primary keys are automatically indexed as btree unique indexes (PostgreSQL default).

### 4.2 Additional Indexes

| Index Name | Table | Column(s) | Unique | Type |
|------------|-------|-----------|--------|------|
| UN_BORDEREAU | bordereau | num_bord | YES | btree |
| UN_UTILISATEUR | utilisateur | nom_utilisateur | YES | btree |
| TOKEN_UN | token | ip | YES | btree |

### 4.3 Toast Tables

Large columns (varchar(2000), text, xml, bytea) are stored in TOAST tables:

| Table | Toast Table | Toasted Columns |
|-------|-------------|-----------------|
| medicament | pg_toast.pg_toast_XXXX | observation (varchar(2000)) |
| signature | pg_toast.pg_toast_XXXX | sign (text) |
| logiciel | pg_toast.pg_toast_XXXX | (binary data) |
| ln | pg_toast.pg_toast_XXXX | (large text columns) |
| facture | pg_toast.pg_toast_XXXX | signature (xml), fact_xml (xml) |

---

## 5. Entity Relationship Diagram

```
                          ┌──────────────┐
                          │   centre     │
                      ┌───┤ code_centre  ├───┐
                      │   └──────────────┘   │
                      │                      │
┌──────────────┐      │   ┌──────────────┐   │   ┌──────────────┐
│ specialite   │      │   │   parametre  │   │   │ utilisateur  │
│ code_sp ─────┼──┐   │   │ code_ps      │   │   │ id_user ─────┼──┐
└──────────────┘  │   │   └──────────────┘   │   └──────────────┘  │
                  │   │                      │                     │
                  │   │   ┌──────────────┐   │   ┌──────────────┐  │
                  │   │   │  bordereau   │   │   │ droit_acces  │  │
                  │   │   │ id_bord (PK) │   │   │ id_user+     │  │
                  │   │   │ num_bord (UQ)│   │   │ composant    │  │
                  │   │   └──────┬───────┘   │   └──────────────┘  │
                  │   │          │           │                     │
                  │   │          │ FK        │                     │ FK
                  │   │          ▼           │                     ▼
                  │   │   ┌──────────────┐   │   ┌──────────────┐
                  │   │   │   facture    │   │   │  ct_acces    │
                  └───┼──→│ num_fact(PK) │←──┼───│ id_user+     │
                      │   │ num_bord(FK) │   │   │ composant    │
                      │   │ code_centre  │   │   └──────────────┘
                      │   │ code_sp(FK)  │   │
                      │   │ id_user(FK)  │───┤
                      │   └──────┬───────┘   │
                      │          │           │
                      │          │ FK        │
                      │          ▼           │
                      │   ┌──────────────┐   │
                      │   │ signature    │   │
                      │   │ num_fact(FK) │   │
                      │   │ sign         │   │
                      │   └──────────────┘   │
                      │                      │
                      │          │ FK        │
                      │          ▼           │
                      │   ┌──────────────┐   │
                      └──→│ detail_fact  │   │
                          │ num_fact(FK) │   │
                          │ num_enr      │   │
                          │ ppa          │   │
                          └──────┬───────┘   │
                                 │           │
                                 │ FK        │
                                 ▼           │
                          ┌──────────────┐   │
                          │ medicament   │   │
                          │ num_enr (PK) │←──┘
                          │ code_forme   │──→forme
                          │ code_sp(FK)  │──→specialite
                          └──────────────┘
```

---

## 6. Critical Data Flows

### 6.1 Invoice Creation Flow

```
parametre.next_num_fact → facture.num_fact (new)
parametre.next_num_bord → bordereau.num_bord (new)

bordereau ← facture (via num_bord FK)
facture ← detail_fact (via num_fact FK)
facture ← signature (via num_fact FK)
detail_fact → medicament (via num_enr lookup)
```

### 6.2 Bordereau Closure Flow

```
cloturerbord(num_bord, ...) → 
  bordereau.etat = 'C' (closed)
  bordereau.date_cloture = now()
  facture.statut_fact = updated
```

### 6.3 Drug Lookup Flow

```
detail_fact.num_enr → medicament.num_enr → 
  medicament.nom_com (display name)
  medicament.tarif_ref (reference price)
  medicament.taux (reimbursement rate)
  medicament.remboursable (eligibility)
```

---

## 7. Missing Relationships (BM Pharma Gaps)

| Gap | Impact | Priority |
|-----|--------|----------|
| No FK from facture to medicament | Drug lookup must be manual | HIGH |
| No FK from detail_fact to medicament | Drug validation not enforced | HIGH |
| No FK from facture to centre | Center validation not enforced | MEDIUM |
| No FK from facture to specialite | Specialty validation not enforced | MEDIUM |
| cm/cm_audit relationships unclear | CM integration uncertain | MEDIUM |
| token/certificat_token relationships unclear | Auth flow uncertain | LOW |

---

## 8. Referential Integrity Analysis

### 8.1 Enforced (Confirmed FKs)

- detail_fact.num_fact → facture.num_fact ✅
- facture.num_bord → bordereau.num_bord ✅
- facture_cm.num_bord → bordereau.num_bord ✅
- droit_acces.id_user → utilisateur.id_user ✅
- signature.num_fact → facture.num_fact ✅

### 8.2 Not Enforced (No FK Constraint)

- facture.code_centre → centre.code_centre ❌
- facture.code_sp → specialite.code_sp ❌
- facture.id_user → utilisateur.id_user ❌
- detail_fact.num_enr → medicament.num_enr ❌
- medic_sp.num_enr → medicament.num_enr ❌
- medic_sp.code_sp → specialite.code_sp ❌
- tarif.num_enr → medicament.num_enr ❌
- medicament.code_forme → forme.code_forme ❌

**Implication**: CHIFA-OFFICINE does not enforce referential integrity at the database level for most relationships. Validation is done at the application level.

---

*Document generated by BM-PHASE-004.9 Real CHIFA Environment & Database Discovery*
*Read-only investigation — NO modifications to CHIFA-OFFICINE files or database*
