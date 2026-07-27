# BM-PHASE-006-D — ANALYSE COMPTEURS PARAMETRE

**Date:** 2026-07-27

---

## Compteurs identifiés

| Compteurs | Type | Valeur actuelle | Format |
|-----------|------|-----------------|--------|
| next_num_fact | int | 1 | varchar(8), padding zeros |
| next_num_bord | int | 215 | varchar(6), padding zeros |

## Comment CHIFA génère les numéros

### next_num_fact
1. CHIFA lit `next_num_fact` depuis `parametre`
2. Formate en 8 caractères avec padding zeros (`00000001`)
3. Utilise ce numéro comme PK de `facture`
4. Incrémente `next_num_fact` de 1

### next_num_bord
1. CHIFA lit `next_num_bord` depuis `parametre`
2. Formate en 6 caractères avec padding zeros (`000215`)
3. Utilise ce numéro comme PK de `bordereau`
4. Incrémente `next_num_bord` de 1

## Risque de concurrence TOCTOU

### Problème
```
Process A: SELECT next_num_fact → 1
Process B: SELECT next_num_fact → 1  ← Même valeur !
Process A: UPDATE next_num_fact = 2
Process B: UPDATE next_num_fact = 2  ← Double attribution !
```

### Solution implémentée
```sql
UPDATE parametre SET next_num_fact = next_num_fact + 1 RETURNING next_num_fact
```
- **Atomique** : SELECT + UPDATE en une seule opération
- **RETURNING** : Retourne la nouvelle valeur directement
- **Pas de TOCTOU** : Impossible de lire la même valeur deux fois

## Comportement après rollback

Si une facture est créée (num_fact attribué) puis rollbackée :
- Le compteur `next_num_fact` a déjà été incrémenté
- Le numéro « perdu » ne sera jamais réutilisé
- **C'est correct** : évite les collisions avec des factures potentiellement déjà envoyées à la CNAS

## Comportement après crash

Si le processus crash après `UPDATE parametre` mais avant `COMMIT` :
- La transaction est automatiquement rollbackée par PostgreSQL
- Le compteur revient à sa valeur d'avant l'UPDATE
- **C'est correct** : pas de numéro attribué sans facture

## Risque de collision

- `next_num_fact` = 1 → format `00000001` (8 chars)
- Espace de numérotation : 100 000 000 numéros (00000000-99999999)
- Collision après ~100M factures → impossible en pratique
- Les numéros TST* (6-8 chars) ne collisionnent pas avec les numéros CHIFA (8 chars)

## Interface implémentée

```csharp
public interface IChifaNumberingService
{
    Task<string> GetNextInvoiceNumberAsync(...);    // UPDATE...RETURNING
    Task<string> GetNextBordereauNumberAsync(...);   // UPDATE...RETURNING
    Task<string> PeekNextInvoiceNumberAsync(...);    // SELECT sans UPDATE
    Task<string> PeekNextBordereauNumberAsync(...);  // SELECT sans UPDATE
}
```

## Recommandation

**Ne jamais modifier manuellement les compteurs.** Utiliser toujours `IChifaNumberingService`.
