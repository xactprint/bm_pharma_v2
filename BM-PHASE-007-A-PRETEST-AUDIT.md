# BM-PHASE-007-A — AUDIT PRÉ-TEST

**Date:** 2026-07-27

---

## Chemin d'exécution exact

Le test réel utilisera uniquement ce chemin :

```
ChifaPostgresInvoiceService.CreateInvoiceAsync(request)
  → ChifaWriteGuard.EnsureWriteAllowedAsync()          [blocage si ReadOnly]
  → ChifaInvoiceValidator.Validate(request)             [validation métier]
  → ChifaWriteDbContext.BeginTransactionAsync()         [transaction PostgreSQL]
  → ChifaWriteDbContext.Factures.Add(facture)           [EF Core INSERT facture]
  → ChifaWriteDbContext.DetailFacts.Add(detail)         [EF Core INSERT detail_fact]
  → ChifaWriteDbContext.SaveChangesAsync()              [SQL généré par EF Core]
  → transaction.CommitAsync()                           [COMMIT]
  → ChifaAuditService.LogOperationAsync()               [audit]
```

### Ce qui ne sera PAS utilisé
- ❌ FakeChifaIntegrationProvider
- ❌ ChifaNumberingService (compteur non modifié — on fournit TST002 explicitement)
- ❌ SQL manuel / Npgsql brut
- ❌ ChifaPostgresBordereauService
- ❌ ChifaSigningService
- ❌ ChifaTokenService
- ❌ OneActionWorkflowService

## Composants impliqués

| Composant | Fichier | Rôle |
|-----------|---------|------|
| ChifaPostgresInvoiceService | Services/ChifaPostgresInvoiceService.cs | Service d'écriture |
| ChifaWriteDbContext | Contexts/ChifaWriteDbContext.cs | DbContext EF Core (write) |
| ChifaWriteGuard | Interfaces/ChifaWriteGuard.cs | Blocage ReadOnly |
| ChifaInvoiceValidator | Services/ChifaInvoiceValidator.cs | Validation |
| ChifaAuditService | Services/ChifaAuditService.cs | Journalisation |
| ChifaFacture | Entities/Chifa/ChifaFacture.cs | Entité facture |
| ChifaDetailFact | Entities/Chifa/ChifaDetailFact.cs | Entité detail_fact |

## Configuration utilisée

| Paramètre | Valeur |
|-----------|--------|
| Mode | Test (pas ReadOnly) |
| ConnectionString | Host=127.0.0.1;Port=5432;Database=CHIFA_OFFICINE;Username=pharm |
| DbContext | ChifaWriteDbContext (Npgsql) |
| Isolation | ReadCommitted |

## Validation du Guard

Le test doit passer en mode `Test` (pas ReadOnly) pour que `ChifaWriteGuard` autorise l'écriture. En mode ReadOnly, le guard lèverait `ChifaWriteBlockedException`.

## Validation du validator

Le validator vérifie :
- num_fact ≤ 8 caractères
- num_assure requis
- code_centre ≤ 5 caractères
- ≥ 1 ligne
- num_enr ≤ 5 caractères
- quantité > 0 et ≤ 999
- PPA > 0
