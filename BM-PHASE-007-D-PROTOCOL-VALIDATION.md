# BM-PHASE-007-D — VALIDATION PROTOCOLE EF CORE

**Date:** 2026-07-27

---

## Résultat

Le code `ChifaPostgresInvoiceService` fonctionne correctement avec EF Core.

### Test InMemory (007-E) — PASSED

| Étape | Résultat |
|-------|----------|
| ChifaWriteGuard (mode Test) | ✅ Autorisé |
| ChifaInvoiceValidator | ✅ Toutes validations passées |
| BeginTransactionAsync | ✅ (InMemory: avertissement supprimé) |
| Factures.Add | ✅ |
| DetailFacts.Add | ✅ |
| SaveChangesAsync | ✅ |
| CommitAsync | ✅ |
| ChifaAuditService.LogOperationAsync | ✅ |

### Données vérifiées

| Champ | Valeur attendue | Valeur réelle |
|-------|----------------|---------------|
| NumFact | TST002 | TST002 ✅ |
| NumAssure | TST99999 | TST99999 ✅ |
| CodeCentre | 11600 | 11600 ✅ |
| MontFact | 120.00 | 120.00 ✅ |
| MontAs | 84.00 | 84.00 ✅ |
| NumEnr | 00010 | 00010 ✅ |
| Qte | 2 | 2 ✅ |
| Ppa | 60.00 | 60.00 ✅ |
| Mont | 120.00 | 120.00 ✅ |

### Protocole validé

Le code EF Core produit les bons types de données, les bons noms de colonnes, et les bons calculs.

**Prêt pour l'écriture réelle sur PostgreSQL.**
