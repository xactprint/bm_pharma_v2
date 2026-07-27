# BM-PHASE-007-B — SNAPSHOT RÉEL READ-ONLY

**Date:** 2026-07-27 21:55 CEST
**PostgreSQL:** 9.3.4 | pharm@CHIFA_OFFICINE

---

## État de la base CHIFA_OFFICINE

| Table | Lignes |
|-------|--------|
| facture | 0 |
| detail_fact | 0 |
| bordereau | 0 |
| signature | 0 |
| ln | 7 412 276 |
| medicament | 7 596 |

## Compteurs

| Paramètre | Valeur |
|-----------|--------|
| next_num_fact | 1 |
| next_num_bord | 215 |
| code_centre | 11600 |
| code_ps | 1234567890 |

## Vérification données TEST

| Recherche | Résultat |
|-----------|----------|
| TST001 | 0 lignes |
| TST* | 0 lignes |

**Aucune donnée test précédente ne subsiste.**

## Baseline pour comparaison

```
facture      = 0
detail_fact  = 0
bordereau    = 0
signature    = 0
next_num_fact = 1
next_num_bord = 215
```

**Snapshot conservé pour comparaison post-écriture et post-rollback.**
