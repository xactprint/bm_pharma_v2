# BM-PHASE-006-E — SÉCURITÉ DES MODES READONLY / TEST / PRODUCTION

**Date:** 2026-07-27

---

## Trois modes

| Mode | DI wiring | WriteGuard | Contexts EF | Compteurs |
|------|-----------|------------|-------------|-----------|
| ReadOnly | FakeChifaIntegrationProvider | Bloque | Non enregistrés | Fake (pas de DB) |
| Test | ChifaPostgresInvoiceService | Autorise | InMemory | Fake (pas de DB) |
| Production | ChifaPostgresInvoiceService | Autorise | Npgsql (réel) | Atomique (SQL) |

## Couches de protection

### Couche 1 : DI Wiring (impossible à contourner)
```
ReadOnly → tous les services sont FakeChifaIntegrationProvider
         → Aucun DbContext n'est enregistré
         → Impossible d'écrire même si le code le veut
```

### Couche 2 : ChifaWriteGuard (blocage explicite)
```csharp
public async Task EnsureWriteAllowedAsync()
{
    var mode = await _getMode();
    if (mode == ChifaIntegrationMode.ReadOnly)
        throw new ChifaWriteBlockedException(...);
}
```
- Appelé en début de chaque service d'écriture
- Exception non catchable silencieusement

### Couche 3 : Services stubs (fallback)
- `ChifaIntegrationServiceStub` : retourne true pour IsChifaAvailable
- `ChifaTokenServiceStub` : retourne false pour IsTokenAvailable
- `ChifaSigningServiceStub` : retourne false pour IsTokenAvailable
- `FakeChifaIntegrationProvider` : implémente tous les interfaces en read-only

## Tests négatifs existants

| Test | Vérifie |
|------|---------|
| REAL010-014 | ReadOnly DI → tous les services sont Fake |
| REAL015-016 | WriteGuard ReadOnly → throw ChifaWriteBlockedException |
| REAL017-018 | WriteGuard Test/Production → pas d'exception |
| GUARD001 | ReadOnly → EnsureWriteAllowedAsync throw |
| GUARD002 | Pas de password en clair dans les logs |

## Risques résiduels

1. **Pas de test pour : WriteGuard ne peut pas être ignoré accidentellement**
   - Mitigation : Le guard est injecté via DI, impossible de le « oublier »

2. **Pas de test pour : ReadOnly ne peut pas appeler CreateInvoiceAsync**
   - Mitigation : Le service n'est même pas enregistré en ReadOnly

3. **Pas de test pour : Le switch de mode est vérifié au démarrage**
   - Mitigation : Le mode est lu depuis la config au démarrage, pas modifiable à l'exécution

## Recommandations

1. Ajouter un test qui prouve que en mode ReadOnly, `ChifaPostgresInvoiceService` ne peut même pas être résolu via DI
2. Ajouter un test qui prouve que `ChifaWriteDbContext` n'est pas enregistré en ReadOnly
3. Documenter que le mode est immutable pendant l'exécution de l'application
