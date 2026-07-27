# BM-PHASE-006-C — STRATÉGIE TRANSACTIONNELLE EF CORE

**Date:** 2026-07-27

---

## Isolation choisie

**ReadCommitted** — isolation par défaut de PostgreSQL.

### Justification
- Pas de besoin de `Serializable` (pas de phantom reads dans notre cas)
- Pas de besoin de `RepeatableRead` (pas de re-read dans la même transaction)
- `ReadCommitted` suffit pour : INSERT facture + INSERT detail_fact + UPDATE facture.NumBord

### Stratégie

```
BEGIN TRANSACTION (ReadCommitted)
  → INSERT facture
  → INSERT detail_fact (1..N lignes)
  → SaveChangesAsync (écrit toutes les mutations)
  → COMMIT
ERREUR → ROLLBACK automatique
```

### Ordre des opérations
1. Vérifier les contraintes métier (validation)
2. Vérifier l'existence du médicament
3. INSERT facture (parent)
4. INSERT detail_fact (enfants)
5. SaveChangesAsync (une seule fois)
6. CommitAsync

### Rollback automatique
- Le `using` sur la transaction garantit le rollback si exception non catchée
- Le bloc `catch` explicit appelle `RollbackAsync`
- Aucune donnée partielle possible

### Gestion des erreurs PostgreSQL
| Erreur | Signification | Action |
|--------|---------------|--------|
| 23503 | FK violation | Médicament inexistant → erreur métier |
| 23505 | PK violation | num_fact déjà existant → erreur métier |
| 23502 | NOT NULL violation | Champ obligatoire manquant → erreur métier |
| 57014 | Query canceled | Timeout → erreur métier |
| 40001 | Serialization failure | Concurrence → retry |

### Risques identifiés
- **Parametre (keyless)** : UPDATE de compteurs via SQL brut, pas via EF Core tracking
- **Concurrence** : Si deux processus écrivent simultanément, le READ COMMITTED suffit car les INSERT sont sur des PK différents
