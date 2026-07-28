# BM-PHASE-008-A — CYCLE DE VIE D'UN BORDEREAU CHIFA

**Document:** 008-A-BORDEREAU-LIFECYCLE
**Date:** 2026-07-28

---

## Structure de la table `bordereau`

| Colonne | Type | Contrainte | Défaut |
|---------|------|-----------|--------|
| `id_bord` | BIGSERIAL | PK | auto |
| `num_bord` | VARCHAR(6) | **UNIQUE** | |
| `code_centre` | VARCHAR(5) | NOT NULL | |
| `etat` | CHAR(1) | | `'O'` (Open) |
| `id_user_cloture` | INTEGER | | NULL |
| `poste_cloture` | VARCHAR(100) | | NULL |
| `mont_vir` | NUMERIC | | 0 |
| `duplicata` | BOOLEAN | | false |
| `date_ouverture` | TIMESTAMP | | `'1900-01-01'` |
| `date_cloture` | TIMESTAMP | | `'1900-01-01'` |
| `date_depot_ftp` | TIMESTAMP | | `'1900-01-01'` |

---

## Workflow d'un bordereau

```mermaid
flowchart TD
    NOUVEAU[Nouveau bordereau] --> SELECT_NEXT[SELECT next_num_bord<br/>FROM parametre]
    SELECT_NEXT --> INSERT_BORD[INSERT INTO bordereau<br/>num_bord='N', etat='O',<br/>code_centre='11600']
    INSERT_BORD --> UPDATE_PRM[UPDATE parametre<br/>SET next_num_bord += 1]
    UPDATE_PRM --> ATTACH[SELECT facture WHERE num_bord IS NULL]
    ATTACH --> LINK[UPDATE facture SET num_bord='N'<br/>WHERE num_fact IN (...)]
    LINK --> VISU[Visualiser Bordereau<br/>FBordereau]
    VISU --> SIGN[SIGNATURE TOKEN]
    SIGN --> CLOSE[cloturerbord()]
    CLOSE --> FTP[TRANSMISSION CNAS]
```

---

## État probabiliste : Visualiser Bordereau

### Comment `FBordereau` charge les bordereaux

L'analyse du code CHIFA-OFFICINE montre que la fenêtre `FBordereau` utilise un
**DataSet nommé `DataSet1`** qui contient une DataTable nommée **`detail_bord`**.

### Il n'existe PAS de table `detail_bord` dans PostgreSQL

C'est une **DataTable en mémoire** construite à partir d'une jointure SQL.

### La jointure présumée pour `detail_bord`

```sql
SELECT f.*, d.*
FROM facture f
JOIN detail_fact d ON f.num_fact = d.num_fact
WHERE f.num_bord = '<numbord>';
```

### Conséquence : visibilité conditionnelle

1. Le bordereau `X` existe dans `bordereau(num_bord='X')`
2. `facture` doit avoir `num_bord='X'` pour être liée
3. `detail_fact` doit avoir `num_fact='<facture num>'` pour que la jointure produise des lignes
4. Si la jointure produit **0 lignes**, `detail_bord` est vide → **bordereau invisible**

### Théorie de l'invisibilité des bordereaux en Phase 005

Dans le test précédent (Phase 005 — TST001), un bordereau a été créé directement
dans PostgreSQL mais il n'apparaissait pas dans Visualiser Bordereau.

**Cause probable :** Le bordereau a été créé, la facture TST001 a été créée,
mais :
- `facture.num_bord` n'a **pas** été mis à jour avec le numéro du bordereau
- OU la jointure `detail_fact` → `facture` a échoué (champ manquant)
- OU le DataSet `detail_bord` filtre avec `qte > 0` ou d'autres conditions

---

## Association facture ↔ bordereau dans BM Pharma

`ChifaPostgresBordereauService.CreateBordereauAsync()` :

```csharp
// Pour chaque invoiceNumbers dans la requête :
var facture = await _context.Factures
    .FirstOrDefaultAsync(f => f.NumFact == numFact);

if (facture != null)
    facture.NumBord = request.NumBord; // ✅ FK update
// Si null → silencieusement ignoré
```

**Problème :** Si la facture n'est pas trouvée (même transaction, pas encore persistée,
ou numéro différent), l'UPDATE est silencieusement ignoré.

---

## Compteurs

```sql
-- Parametre.next_num_bord est le compteur unique
SELECT next_num_bord FROM parametre;  -- Actuellement: 215

-- Atomicité via UPDATE...RETURNING
UPDATE parametre SET next_num_bord = next_num_bord + 1
RETURNING next_num_bord;
```

**Design BM Pharma actuel :**
- `ChifaNumberingService.GetNextBordereauNumberAsync()` — atomique, non intégré
- `ChifaPostgresBordereauService.CreateBordereauAsync()` — prend `NumBord` de la requête
- L'appelant doit demander le nombre puis passer au service
- Entre les deux appels : **fenêtre de concurrence** ouverte

---

## Ce qui doit être testé en 008-B

1. Créer un bordereau (`num_bord` généré par CHIFA ou explicite)
2. Créer une facture avec `num_bord` lié au bordereau
3. Vérifier « Visualiser Bordereau » dans CHIFA-OFFICINE
4. Vérifier visibilité dans « Consultation Facture »
