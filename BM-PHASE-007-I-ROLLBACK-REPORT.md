# BM-PHASE-007-I — ROLLBACK CONTRÔLÉ TST002

**Date:** 2026-07-27 20:26 UTC
**Résultat:** ✅ ROLLBACK COMPLET — BASELINE RESTAURÉE

---

## 1. État AVANT rollback

| Table | Lignes |
|-------|--------|
| facture | 1 (TST002) |
| detail_fact | 1 (TST002-00010) |
| bordereau | 0 |
| signature | 0 |

| Compteur | Valeur |
|----------|--------|
| next_num_fact | 1 |
| next_num_bord | 215 |

---

## 2. Procédure de rollback

```sql
BEGIN TRANSACTION;
DELETE FROM detail_fact WHERE num_fact = 'TST002';  -- 1 row deleted
DELETE FROM facture WHERE num_fact = 'TST002';       -- 1 row deleted
COMMIT;
```

Ordre respecté : detail_fact (FK enfant) puis facture (FK parent).

---

## 3. État APRÈS rollback

| Table | Lignes | Attendu | Statut |
|-------|--------|---------|--------|
| facture | 0 | 0 | ✅ |
| detail_fact | 0 | 0 | ✅ |
| bordereau | 0 | 0 | ✅ |
| signature | 0 | 0 | ✅ |
| ln | 7 412 276 | 7 412 276 | ✅ |
| medicament | 7 596 | 7 596 | ✅ |

| Compteur | Avant | Après | Statut |
|----------|-------|-------|--------|
| next_num_fact | 1 | 1 | ✅ |
| next_num_bord | 215 | 215 | ✅ |
| code_centre | 11600 | 11600 | ✅ |
| code_ps | 1234567890 | 1234567890 | ✅ |

---

## 4. Validation finale — 10/10 checks

| # | Check | Résultat |
|---|-------|----------|
| 1 | facture = 0 | ✅ |
| 2 | detail_fact = 0 | ✅ |
| 3 | bordereau = 0 | ✅ |
| 4 | signature = 0 | ✅ |
| 5 | next_num_fact = 1 | ✅ |
| 6 | next_num_bord = 215 | ✅ |
| 7 | ln = 7 412 276 | ✅ |
| 8 | medicament = 7 596 | ✅ |
| 9 | TST002 facture gone | ✅ |
| 10 | TST002 detail_fact gone | ✅ |

---

## 5. Baseline restaurée

| Table | Baseline | Post-Rollback | Identique |
|-------|----------|---------------|-----------|
| facture | 0 | 0 | ✅ |
| detail_fact | 0 | 0 | ✅ |
| bordereau | 0 | 0 | ✅ |
| signature | 0 | 0 | ✅ |
| next_num_fact | 1 | 1 | ✅ |
| next_num_bord | 215 | 215 | ✅ |

**La base est exactement dans l'état où elle était avant Phase 007-G.**

---

## CONCLUSION

**ROLLBACK COMPLET — BASELINE RESTAURÉE**

Aucune donnée ne subsiste de TST002. La base CHIFA_OFFICINE est revenue à son état initial.

**⛔ STOP — Phase 007 terminée. En attente d'approbation pour une nouvelle phase.**
