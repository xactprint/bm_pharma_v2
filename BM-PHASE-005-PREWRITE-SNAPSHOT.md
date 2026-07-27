# BM-PHASE-005-A — PRE-WRITE SNAPSHOT

**Date:** 2026-07-27 21:09 CEST
**Mode:** READ-ONLY — AUCUNE ÉCRITURE EFFECTUÉE
**PostgreSQL:** 9.3.4 | pharm@CHIFA_OFFICINE

---

## État initial de la base CHIFA_OFFICINE

| Table | Lignes |
|-------|--------|
| facture | 0 |
| detail_fact | 0 |
| bordereau | 0 |
| parametre | 1 |
| medicament | 7 596 |
| signature | 0 |
| ln | 7 412 276 |

## Compteurs critiques

| Paramètre | Valeur |
|-----------|--------|
| next_num_fact | 1 |
| next_num_bord | 215 |
| code_centre | 11600 |
| code_ps | 1234567890 |
| annee | 2024 |
| version | 3.0.4.3 |

## FK Integrity

- Orphan factures (no bordereau): **0**
- Orphan detail_fact (no facture): **0**
- Orphan signatures (no facture): **0**

## Structure critique

| Table | Colonnes | PK | NOT NULL |
|-------|----------|-----|----------|
| facture | 53 | num_fact | num_fact, type_maj |
| detail_fact | 20 | (num_fact, num_enr, ppa) | num_fact, num_enr, qte, ppa, mont, num_enr_prescrit |
| bordereau | 11 | num_bord | num_bord, code_centre |
| parametre | 58 | code_ps | code_ps |
| medicament | 29 | num_enr | num_enr |
| signature | 2 | num_fact | num_fact |

## Constraints

- `facture.num_fact`: varchar(8), PK, NOT NULL
- `facture.num_bord`: varchar(6), FK → bordereau.num_bord, NULLABLE
- `facture.type_maj`: integer, NOT NULL, no default
- `detail_fact.num_fact`: varchar(8), FK → facture.num_fact, NOT NULL
- `detail_fact.num_enr`: varchar(5), NOT NULL, must exist in medicament
- `detail_fact.num_enr_prescrit`: varchar(5), NOT NULL
- `bordereau.num_bord`: varchar(6), PK + UNIQUE, NOT NULL

## Medicament utilisé pour le test

| Champ | Valeur |
|-------|--------|
| num_enr | 00010 |
| nom_com | ZYRTEC |
| tarif_ref | 60.00 |
| convention | O |
| remboursable | O |
| code_forme | 143 |

## Snapshot

```
facture      = 0
detail_fact  = 0
bordereau    = 0
parametre    = 1
medicament   = 7596
signature    = 0
ln           = 7412276
next_num_fact = 1
next_num_bord = 215
```

**Aucune modification n'a été effectuée.**
