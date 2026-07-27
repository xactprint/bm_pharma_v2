# BM-PHASE-004.12-SCHEMA-DISCOVERY.md
# Phase 2: Full Schema Inventory

**Date:** 2026-07-27
**Status:** ✅ COMPLETE

---

## Schema Overview

| Metric | Count |
|--------|-------|
| Tables | 48 |
| Primary Keys | 47 |
| Foreign Keys | 5 |
| Indexes | 34 |
| Sequences | 2 |
| Constraints | 107 |
| Functions | 51 |

## All 48 Tables

```
attestation_mc    beneficiaire       bordereau          carte_chifa
centre             certificat_token   cm                 cm_audit
condition          conditionnement    detail_fact        detail_fact2
detail_fact_cm     detail_fact_cm2    droit_acces        facture
facture2           facture_cm         facture_cm2        file
forme              liste_noire        liste_noire2       liste_noire_light
ln                 logiciel           medic_demuni       medic_ppa
medic_sp           medicament         medicament2        morfine
mutualiste_radie   parametre          parametre_code_barre  rupture_stock
signature          specialite         tarif              temp
temp00             temp01             temp02             temp03
temp04             token              type_posologie     utilisateur
```

## Sequences (2)

| Sequence Name | Purpose |
|---------------|---------|
| bordereau_id_bord_seq | Auto-increment for bordereau.id_bord |
| utilisateur_id_user_seq | Auto-increment for utilisateur.id_user |

## Functions (51)

Core functions:
- `add` — Generic add/insert helper
- `cloturerbord` (x2 overloads) — Close a bordereau
- `describe_table` — Table metadata introspection
- `importdata` / `importtable` — Data import utilities
- `increment` — Sequence increment helper
- `readfile` — File reading utility

Infrastructure:
- `dblink` functions — Remote query execution via dblink extension
- `pg_stat_statements` — Query statistics (extension)

## Foreign Key Relationships

| Child Table | FK Column | Parent Table | Parent PK |
|-------------|-----------|--------------|-----------|
| facture | num_bord | bordereau | num_bord |
| detail_fact | num_fact | facture | num_fact |
| signature | num_fact | facture | num_fact |
| detail_fact_cm | num_fact | facture_cm | num_fact |
| detail_fact_cm2 | num_fact | facture_cm2 | num_fact |

## Key Observations

1. **50% of tables are empty or temp** — Many temp* tables and facture2/detail_fact2 copies exist as legacy artifacts
2. **Singleton pattern** — `parametre` has exactly 1 row with flat columns (not key/value)
3. **Lightweight FK usage** — Only 5 foreign keys in 48 tables; most tables are loosely coupled
4. **Large ln table** — 7.4M rows for serial number tracking (the largest table by far)
5. **Dual facture systems** — facture/facture2 and detail_fact/detail_fact2 exist side by side; facture_cm/facture_cm2 for CM-specific billing
