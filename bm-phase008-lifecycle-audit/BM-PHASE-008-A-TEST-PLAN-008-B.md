# BM-PHASE-008-A — PLAN DE TEST POUR PHASE 008-B

**Document:** 008-A-TEST-PLAN-008-B
**Date:** 2026-07-28

---

## Objectif

Tester la **visibilité réelle** d'une facture et d'un bordereau dans CHIFA-OFFICINE,
et confirmer/infirmer la théorie de la jointure `detail_bord` pour « Visualiser Bordereau ».

---

## Données de test

### Bordereau

| Champ | Valeur | Commentaire |
|-------|--------|-------------|
| `num_bord` | `TST003` | 6 chars ≤ 6 max |
| `code_centre` | `11600` | Centre de test |
| `etat` | `O` | Ouvert |
| `date_ouverture` | `NOW()` | Aujourd'hui |

### Facture

| Champ | Valeur | Commentaire |
|-------|--------|-------------|
| `num_fact` | `TST003` | Même que bordereau pour clarté |
| `num_assure` | `TST99999` | Test |
| `code_centre` | `11600` | Centre |
| `num_bord` | `TST003` | **Lié au bordereau** |
| `mont_fact` | 120.00 | 2 × ZYRTEC |
| `mont_as` | 84.00 | 70% |
| `mont_off` | 120.00 | = mont_fact |
| `taux` | `3` | |
| `type_maj` | 0 | |
| `nat_remb` | `0` | **Fixé explicitement** (non NULL) |
| `mont_mut` | 0 | **Fixé explicitement** (non NULL) |
| `date_fin_mut` | `'1900-01-01'` | **Fixé explicitement** (non NULL) |
| `date_synchro` | `'1900-01-01'` | **Fixé explicitement** (non NULL) |
| autres champs | cf TST002 | Mêmes valeurs |

### Detail_fact

| Champ | Valeur | Commentaire |
|-------|--------|-------------|
| `num_fact` | `TST003` | Lié à la facture |
| `num_enr` | `00010` | ZYRTEC |
| `qte` | 2 | Quantité |
| `ppa` | 60.00 | Prix unitaire |
| `mont` | 120.00 | Total ligne |
| `mont_as` | 84.00 | 70% |
| `mont_pharm` | 36.00 | 30% |
| `remboursable` | `true` | **Fixé explicitement** |
| autres champs | cf TST002 | Mêmes valeurs |

---

## Protocole pas à pas

### Phase 008-B-1 : Préparation (READ-ONLY)

```
1. Vérifier baseline :
   - facture = 0
   - detail_fact = 0
   - bordereau = 0
   - signature = 0
   - next_num_fact = 1
   - next_num_bord = 215

2. Vérifier que TST003 n'existe pas :
   - SELECT count(*) FROM facture WHERE num_fact = 'TST003'  → 0
   - SELECT count(*) FROM detail_fact WHERE num_fact = 'TST003' → 0
   - SELECT count(*) FROM bordereau WHERE num_bord = 'TST003'  → 0

3. Vérifier connexion PostgreSQL 9.3.4, pharm@CHIFA_OFFICINE
```

### Phase 008-B-2 : Écriture bordereau (INSERT direct via Npgsql)

```
1. BEGIN TRANSACTION
2. INSERT INTO bordereau(num_bord, code_centre, etat, date_ouverture)
   VALUES ('TST003', '11600', 'O', NOW());
3. INSERT INTO facture(...) VALUES('TST003', ...)  -- avec num_bord='TST003'
4. INSERT INTO detail_fact(...) VALUES('TST003', '00010', ...)
5. COMMIT
6. Ne PAS modifier parametre
```

**⚠️ Étape 2-4 en une seule transaction.**

### Phase 008-B-3 : Vérification écriture

```
1. SELECT vérification de toutes les données
2. Vérifier jointure :
   SELECT f.*, b.*
   FROM facture f JOIN bordereau b ON f.num_bord = b.num_bord
   WHERE f.num_fact = 'TST003';
   → 1 ligne
```

### Phase 008-B-4 : ⛔ STOP — Ouvrir CHIFA-OFFICINE

```
1. NE PAS fermer le terminal
2. Ouvrir CHIFA-OFFICINE manuellement
3. Vérifier :
   a. Consultation Facture → chercher TST003 → visible ?
   b. Visualiser Bordereau → chercher TST003 → visible ?
4. Documenter les résultats
5. Revenir au terminal
6. Continuer
```

### Phase 008-B-5 : Vérification complémentaire (optionnelle)

```
SI le bordereau est invisible :
  1. Vérifier que detail_fact a bien qte > 0
  2. Vérifier que detail_fact.remboursable = true
  3. Vérifier nat_remb, mont_mut, date_synchro VALUES
  4. Si nécessaire : ajouter 2ème ligne à detail_fact

SI toujours invisible :
  1. Théorie confirmée : detail_bord a des conditions supplémentaires
  2. Pas possible de déterminer sans décompiler FBordereau
```

### Phase 008-B-6 : ⛔ STOP APRÈS VÉRIFICATION

```
1. Produire le rapport BM-PHASE-008-B-REPORT.md
2. STOP — En attente d'approbation pour rollback
```

### Phase 008-B-7 : Rollback (après approbation)

```
BEGIN TRANSACTION
DELETE FROM detail_fact WHERE num_fact = 'TST003'
DELETE FROM facture WHERE num_fact = 'TST003'
DELETE FROM bordereau WHERE num_bord = 'TST003'
COMMIT

Vérifier baseline restaurée : tout à 0
```

---

## Résumé des INSERT

```sql
-- 1. Bordereau
INSERT INTO bordereau(num_bord, code_centre, etat, date_ouverture)
VALUES ('TST003', '11600', 'O', NOW());

-- 2. Facture
INSERT INTO facture(
    num_fact, date_fact, etat, num_bord,
    mont_off, mont_as, mont_fact,
    num_assure, rang_ad, code_centre,
    tp, taux, code_affect, conv, type_consult,
    prescripteur, date_soin, risque,
    statut_fact, verifcms, type_signature, verif_fact,
    mont_maj_fae, mont_maj, type_maj,
    nat_remb, mont_mut, code_mut, date_fin_mut, date_synchro,
    version, echifa, e_ord
) VALUES (
    'TST003', NOW(), '0', 'TST003',
    120.00, 84.00, 120.00,
    'TST99999', '1', '11600',
    '1', '3', '01', '1', '01',
    'BM PHARMA', CURRENT_DATE, '0',
    '1', '0', '0', '0',
    0, 0, 0,
    '0', 0, NULL, '1900-01-01', '1900-01-01',
    '2.0.0', false, false
);

-- 3. Detail_fact
INSERT INTO detail_fact(
    num_fact, num_enr, ppa, qte, mont, mont_as, mont_pharm,
    num_enr_prescrit, num_lot, duree_trait, tarif_ref, posologie,
    remboursable, local, inf_tr, applic_tr, medic, ts,
    maj_local, maj_sub
) VALUES (
    'TST003', '00010', 60.00, 2, 120.00, 84.00, 36.00,
    '00010', 'LOT003', 30, 60.00, '1/day',
    true, false, true, true, true, false,
    0, 0
);
```

---

## Risques

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| Bordereau invisible dans Visualiser | Élevée (¯\_(ツ)_/¯) | Moyen | Théorie documentée, pas bloquant |
| CHIFA-OFFICINE crash en lisant les données | Faible | Moyen | Rollback immédiat |
| Conflit num_fact avec CHIFA | Très faible | Faible | Utiliser TST003 (hors plage CHIFA) |
| `next_num_fact` modifié par inadvertance | Faible | Faible | Ne PAS utiliser de compteur |
| Données visibles par erreur dans production | Très faible | Faible | Rollback immédiat après test |

---

## Critères d'arrêt immédiat

1. CHIFA-OFFICINE crée des données involontaires (signature, cloture)
2. Le token professionnel est sollicité
3. Une fenêtre `cloturerbord` s'ouvre
4. Une tentative de transmission CNAS est détectée
5. `next_num_fact` ou `next_num_bord` est modifié

---

## Résultat attendu

| Scénario | Probabilité | Explication |
|----------|-------------|-------------|
| Facture visible + Bordereau visible | 30% | Si `detail_bord` ne fait que JOIN sans filtre supplémentaire |
| Facture visible + Bordereau invisible | 60% | Si `detail_bord` a condition supplémentaire (qte > 0, remboursable = true, etc.) |
| Facture invisible + Bordereau invisible | 10% | Si CHIFA filtre aussi par `code_centre` ou autre |

Le plus probable : **Facture visible, Bordereau invisible** → confirme que
`detail_bord` a des conditions supplémentaires non couvertes par notre INSERT.
