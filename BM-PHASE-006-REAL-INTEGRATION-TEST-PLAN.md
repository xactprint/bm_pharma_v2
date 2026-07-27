# BM-PHASE-006-REAL-INTEGRATION-TEST-PLAN — PROTOCOLE D'ÉCRITURE RÉELLE

**Date:** 2026-07-27
**Statut:** PRÉPARÉ — NE PAS EXÉCUTER SANS APPROBATION EXPLICITE

---

## Identifiant de test

`TST002` (différent de TST001 utilisé en Phase 005)

## Prérequis

1. PostgreSQL CHIFA_OFFICINE en cours d'exécution
2. Baseline vérifiée : facture=0, detail_fact=0, bordereau=0, signature=0
3. Mode = Test ou Production (pas ReadOnly)

## Protocole

### Étape 1 : Snapshot avant
```sql
SELECT count(*) FROM facture;
SELECT count(*) FROM detail_fact;
SELECT count(*) FROM bordereau;
SELECT count(*) FROM signature;
SELECT next_num_fact, next_num_bord, code_centre FROM parametre;
```

### Étape 2 : Écriture via EF Core
- Utiliser `ChifaPostgresInvoiceService.CreateInvoiceAsync`
- num_fact = `TST002`
- medicament = `00010` (ZYRTEC)
- qte = 1, ppa = 60.00, mont = 60.00
- code_centre = `11600`
- num_assure = `TST99999`

### Étape 3 : Vérification SQL
```sql
SELECT * FROM facture WHERE num_fact = 'TST002';
SELECT * FROM detail_fact WHERE num_fact = 'TST002';
SELECT count(*) FROM facture;
SELECT count(*) FROM detail_fact;
```

### Étape 4 : Vérification CHIFA UI
- Ouvrir CHIFA-OFFICINE
- Vérifier que TST002 apparaît dans la liste des factures

### Étape 5 : Rollback
```sql
BEGIN;
DELETE FROM detail_fact WHERE num_fact = 'TST002';
DELETE FROM facture WHERE num_fact = 'TST002';
COMMIT;
```

### Étape 6 : Snapshot après
```sql
SELECT count(*) FROM facture;
SELECT count(*) FROM detail_fact;
SELECT count(*) FROM bordereau;
SELECT count(*) FROM signature;
SELECT next_num_fact, next_num_bord, code_centre FROM parametre;
```

### Étape 7 : Comparaison
Vérifier que :
- facture = 0 (baseline)
- detail_fact = 0 (baseline)
- bordereau = 0 (baseline)
- signature = 0 (baseline)
- next_num_fact = 2 (incrementé de 1)
- next_num_bord = 215 (inchangé)
- code_centre = 11600 (inchangé)

## Risques

| Risque | Impact | Mitigation |
|--------|--------|------------|
| CHIFA UI affiche TST002 | Visible | Rollback immédiat après vérification |
| Compteur incrementé | next_num_fact = 2 | Attendu, pas de rollback du compteur |
| Erreur FK | Échec écriture | Rollback automatique |
| Timeout | Échec écriture | Rollback automatique |

## Résultat attendu

- Écriture : SUCCÈS
- Vérification UI : TST002 visible
- Rollback : COMPLET
- Compteur : next_num_fact = 2 (post-rollback)
- Tous les tests automatisés : 509/509 passent
