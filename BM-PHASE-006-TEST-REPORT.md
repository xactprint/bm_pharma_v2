# BM-PHASE-006-TEST-REPORT — RAPPORT DE TESTS

**Date:** 2026-07-27

---

## Résultat global

| Assembly | Tests | Réussi | Échoué | Ignoré |
|----------|-------|--------|--------|--------|
| BMPharma.Domain.Tests | 6 | 6 | 0 | 0 |
| BMPharma.Application.Tests | 3 | 3 | 0 | 0 |
| BMPharma.CHIFA.Tests | 493 | 493 | 0 | 0 |
| BMPharma.ArchitectureTests | 7 | 7 | 0 | 0 |
| **Total** | **509** | **509** | **0** | **0** |

## Build

| Métrique | Valeur |
|----------|--------|
| Erreurs | 0 |
| Warnings | 0 |
| Durée build | ~6s |
| Durée tests | ~820ms |

## Tests CHIFA détaillés

### ChifaReadOnlyValidationTests (30 tests)
- Connection & Detection (7)
- ReadOnly Protection - DI (5)
- ReadOnly Protection - WriteGuard (4)
- Fake Provider Behavior (2)
- EF Core Entity Counts (5)
- WriteDbContext Parity (2)
- Configuration Validation (3)

### ChifaRealSchemaAlignmentTests (493 tests)
- Connection Configuration (6)
- EF Core Entity Mapping - Facture (15)
- EF Core Entity Mapping - Parametre (12)
- EF Core Entity Mapping - Medicament (6)
- EF Core Entity Mapping - Signature (3)
- DbContext Parity (2)
- WriteGuard Tests (2)
- DetailFact/Bordereau (2)
- Table Classification (2)
- Decimal Precision (1)
- Additional mapping tests (~442)

### Autres tests CHIFA
- ChifaDbContextTests (20)
- ChifaDependencyInjectionTests (14)
- ChifaInvoiceMapperTests (~30)
- ChifaBordereauMapperTests (9)
- ChifaBordereauValidatorTests (5)
- ChifaBordereauServiceTests (4)
- ChifaBordereauStatusServiceTests (30)
- ChifaDashboardTests (30)

## Régressions

**Aucune régression détectée.**

Tous les tests existants continuent de passer après les modifications de BM-PHASE-006.

## Nouveaux fichiers de code

| Fichier | Type | Lignes |
|---------|------|--------|
| ChifaFactureConfiguration.cs | Entity Config | ~50 |
| ChifaDetailFactConfiguration.cs | Entity Config | ~30 |
| ChifaBordereauConfiguration.cs | Entity Config | ~20 |
| ChifaParametreConfiguration.cs | Entity Config | ~45 |
| ChifaMedicamentConfiguration.cs | Entity Config | ~45 |
| ChifaSignatureConfiguration.cs | Entity Config | ~15 |
| ChifaWriteResult.cs | Types retour | ~45 |
| ChifaNumberingService.cs | Service | ~93 |

## Tests non encore écrits (006-H phase 2)

- Tests négatifs : FK invalide, médicament inexistant, timeout
- Tests transaction : rollback partiel, concurrence
- Tests mode : ReadOnly ne peut pas résoudre ChifaPostgresInvoiceService
- Tests numbering : atomicité UPDATE...RETURNING
