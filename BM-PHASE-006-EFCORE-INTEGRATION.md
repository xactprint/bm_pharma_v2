# BM-PHASE-006-EFCORE-INTEGRATION — INTÉGRATION EF CORE

**Date:** 2026-07-27

---

## Architecture EF Core

```
ChifaPostgreSqlContext (READ-ONLY)
  └─ ChifaFactureConfiguration (shared)
  └─ ChifaDetailFactConfiguration (shared)
  └─ ChifaBordereauConfiguration (shared)
  └─ ChifaParametreConfiguration (shared)
  └─ ChifaMedicamentConfiguration (shared)
  └─ ChifaSignatureConfiguration (shared)

ChifaWriteDbContext (WRITE)
  └─ ChifaFactureConfiguration (shared)
  └─ ChifaDetailFactConfiguration (shared)
  └─ ChifaBordereauConfiguration (shared)
  └─ ChifaParametreConfiguration (shared)
  └─ ChifaMedicamentConfiguration (shared)
  └─ ChifaSignatureConfiguration (shared)
```

## Configurations extraites (6 fichiers)

| Fichier | Table | PK | Notes |
|---------|-------|-----|-------|
| ChifaFactureConfiguration.cs | facture | NumFact | 53 props, 27 MaxLength, 6 decimal |
| ChifaDetailFactConfiguration.cs | detail_fact | (NumFact,NumEnr,Ppa) | Composite PK, 20 props |
| ChifaBordereauConfiguration.cs | bordereau | IdBord | Unique index sur NumBord |
| ChifaParametreConfiguration.cs | parametre | HasNoKey | 58 props, singleton |
| ChifaMedicamentConfiguration.cs | medicament | NumEnr | 29 props, 7596 rows |
| ChifaSignatureConfiguration.cs | signature | NumFact | 2 props, 0 rows |

## Mapping corrigé

### ChifaBordereau
- **Avant :** PK = IdBord (auto-increment), pas d'index sur NumBord
- **Après :** PK = IdBord + Index unique `UN_BORDEREAU` sur NumBord
- **Impact :** NumBord est maintenant vérifié comme UNIQUE par EF Core

### Contextes simplifiés
- **Avant :** 180 lignes de configuration inline dans chaque DbContext
- **Après :** 20 lignes avec `ApplyConfigurationsFromAssembly`
- **Avantage :** Modification unique, pas de duplication

## Fichiers modifiés

| Fichier | Action |
|---------|--------|
| ChifaPostgreSqlContext.cs | Simplifié à 20 lignes |
| ChifaWriteDbContext.cs | Simplifié à 20 lignes |
| ChifaFactureConfiguration.cs | Nouveau |
| ChifaDetailFactConfiguration.cs | Nouveau |
| ChifaBordereauConfiguration.cs | Nouveau (avec index unique) |
| ChifaParametreConfiguration.cs | Nouveau |
| ChifaMedicamentConfiguration.cs | Nouveau |
| ChifaSignatureConfiguration.cs | Nouveau |

## Validation

- Build : 0 erreurs, 0 warnings
- Tests : 509/509 passent
- Aucune régression
