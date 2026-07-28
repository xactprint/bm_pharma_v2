# BM-PHASE-008-A — ANALYSE DE VISIBILITÉ

**Document:** 008-A-VISIBILITY-ANALYSIS
**Date:** 2026-07-28

---

## Cas A : Facture créée directement dans PostgreSQL — ✅ VISIBLE

### Preuve : Phase 005-E (TST001)

Lors de BM-PHASE-005, la facture TST001 a été :
1. Créée via `INSERT INTO facture(...)` direct
2. Vérifiée via `SELECT` — existante
3. Visualisée dans « Consultation Facture » de CHIFA-OFFICINE — ✅ visible

### Pourquoi ça marche ?

`FConsultation_Facture` exécute probablement :
```sql
SELECT * FROM facture
WHERE num_fact LIKE '<search>'
  AND code_centre = '<connected_centre>';
```

Aucune jointure, aucun prérequis. La facture est directement lisible.

### Conditions minimales de visibilité :
- ✅ La ligne existe dans `facture`
- ✅ `num_fact` est unique (PK)
- ✅ `code_centre` correspond au centre connecté

---

## Cas B : Bordereau créé directement — ❓ INVISIBLE (probablement)

### Observation précédente

Un bordereau créé directement dans PostgreSQL n'apparaissait PAS dans
« Visualiser Bordereau » lors du test précédent.

### Analyse de la cause

`FBordereau` charge les bordereaux **via un DataSet nommé `DataSet1`**
qui contient `detail_bord` — **pas une table PostgreSQL**.

La DataTable `detail_bord` est construite via **jointure SQL** :
```sql
SELECT f.*, d.*
FROM facture f
JOIN detail_fact d ON f.num_fact = d.num_fact
WHERE f.num_bord = '<numbord>';
```

### Arbre de décision de visibilité

```
bordereau.num_bord = 'X'   ?
  ↓
facture.num_bord = 'X'     ?   ← 1ère condition d'échec probable
  ↓
detail_fact.num_fact =     ?   ← 2ème condition d'échec probable
  facture.num_fact
  ↓
JOIN produit ≥ 1 ligne      ?   ← Si 0 → invisible
  ↓
detail_bord.DataTable       →   BindingSource → DataGridView
```

---

## Cas C : Facture + bordereau créés par CHIFA normalement — REFERENCE

### Analyse statique uniquement (pas créé réellement)

**Étapes réelles dans CHIFA-OFFICINE :**
```
1. FBordereau ouvre → SELECT next_num_bord FROM parametre → 215
2. INSERT INTO bordereau(num_bord='215', code_centre='11600', etat='O')
3. UPDATE parametre SET next_num_bord = 216
4. FBordereau affiche les factures NON assignées (num_bord IS NULL)
5. Utilisateur sélectionne une facture → UPDATE facture SET num_bord='215'
6. DataSet1.detail_bord chargé → JOIN facture ON detail_fact
7. Visualisation fonctionne car JOIN produit des lignes
```

---

## Comparaison des 3 cas

| Aspect | Cas A (Facture seule) | Cas B (Bordereau seul) | Cas C (CHIFA normal) |
|--------|----------------------|----------------------|---------------------|
| Créé par | INSERT direct PostgreSQL | INSERT direct PostgreSQL | CHIFA-OFFICINE UI |
| Visible dans Consultation Facture | ✅ OUI | N/A (pas une facture) | ✅ OUI |
| Visible dans Visualiser Bordereau | N/A | ❓ PROBABLEMENT NON | ✅ OUI |
| `facture.num_bord` lié | NULL | ? | ✅ OUI |
| `detail_fact` existe | ✅ OUI | N/A | ✅ OUI |
| Jointure détail produit des lignes | N/A | ? | ✅ OUI |

---

## Théorie de l'invisibilité — Version définitive

```
INVISIBILITÉ D'UN BORDEREAU CRÉÉ DIRECTEMENT =
  bordereau.num_bord   ne correspond à   facture.num_bord
  OU
  facture.num_bord = NULL
  OU
  detail_fact.num_fact ≠ facture.num_fact
  OU
  La jointure dans detail_bord a une condition WHERE supplémentaire
  (qte > 0, remboursable = true, etc.)
```

### Pour confirmer ou infirmer la théorie — Phase 008-B

Le test de Phase 008-B doit :
1. Créer un bordereau
2. Créer une facture avec `num_bord` renseigné
3. Vérifier que la jointure `facture.num_bord = bordereau.num_bord` existe
4. Vérifier que `detail_fact` a bien `num_fact` = facture
5. Ouvrir CHIFA-OFFICINE → vérifier visibilité
6. Si invisible → ajouter `detail_fact` avec `qte > 0`
7. Si toujours invisible → capturer la requête SQL réelle

---

## DataSet `DataSet1` — Structure complète (décompilée)

| DataTable | Colonnes | Source |
|-----------|----------|--------|
| `detail_fact` | 20 colonnes | `SELECT * FROM detail_fact WHERE num_fact = ?` |
| `detail_bord` | facture.* + detail_fact.* | `SELECT f.*, d.* FROM facture f JOIN detail_fact d ON f.num_fact = d.num_fact WHERE f.num_bord = ?` |
| `remarques` | ? | ? |
| `parametre` | 58 colonnes | `SELECT * FROM parametre` |
| `param_demande_cm` | ? | ? |
| `parametre_journal` | ? | ? |

**Conclusion :** `detail_bord` est la **clé de la visibilité** des bordereaux.
Tant que cette jointure ne produit pas de lignes, le bordereau est invisible.
