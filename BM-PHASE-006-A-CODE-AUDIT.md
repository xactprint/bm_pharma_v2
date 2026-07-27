# BM-PHASE-006-A — AUDIT DU CODE EXISTANT

**Date:** 2026-07-27
**Statut:** READ-ONLY — Aucune modification effectuée

---

## 1. INVENTAIRE DES FICHIERS ANALYSÉS

### Contextes EF Core
| Fichier | Lignes | Rôle |
|---------|--------|------|
| ChifaPostgreSqlContext.cs | 180 | DbContext READ-ONLY |
| ChifaWriteDbContext.cs | 180 | DbContext WRITE |

### Entités
| Fichier | Lignes | Table | Colonnes |
|---------|--------|-------|----------|
| ChifaFacture.cs | 171 | facture | 53 |
| ChifaDetailFact.cs | 67 | detail_fact | 20 |
| ChifaBordereau.cs | 40 | bordereau | 11 |
| ChifaParametre.cs | 186 | parametre | 58 |
| ChifaMedicament.cs | 99 | medicament | 29 |
| ChifaSignature.cs | 18 | signature | 2 |

### Services
| Fichier | Lignes | Statut |
|---------|--------|--------|
| ChifaPostgresInvoiceService.cs | 116 | STUB — transaction vide |
| ChifaPostgresBordereauService.cs | 127 | STUB — transaction vide |
| OneActionWorkflowService.cs | 409 | Fonctionnel (orchestration) |
| ChifaInvoiceMapper.cs | 137 | Fonctionnel |
| ChifaBordereauMapper.cs | 44 | Fonctionnel |
| ChifaInvoiceValidator.cs | 123 | Fonctionnel |
| ChifaBordereauValidator.cs | 52 | Fonctionnel |

### Sécurité
| Fichier | Lignes | Statut |
|---------|--------|--------|
| ChifaWriteGuard.cs | 39 | ✅ Fonctionnel |
| ChifaIntegrationModeProvider.cs | 26 | ✅ Fonctionnel |
| ChifaEnums.cs | 12 | ✅ Fonctionnel |

### Infrastructure
| Fichier | Lignes | Statut |
|---------|--------|--------|
| DependencyInjection.cs | 118 | ✅ Fonctionnel |
| ChifaIntegrationConfig | (dans ChifaEnums.cs) | ✅ Fonctionnel |

### Tests
| Fichier | Tests |
|---------|-------|
| ChifaReadOnlyValidationTests.cs | 30 |
| ChifaRealSchemaAlignmentTests.cs | 493 |
| ChifaDbContextTests.cs | 20 |
| ChifaDependencyInjectionTests.cs | 14 |
| ChifaInvoiceMapperTests.cs | ~30 |
| ChifaBordereauMapperTests.cs | 9 |
| ChifaBordereauValidatorTests.cs | 5 |
| ChifaBordereauServiceTests.cs | 4 |
| ChifaBordereauStatusServiceTests.cs | 30 |
| ChifaDashboardTests.cs | 30 |
| **Total** | **~665** |

---

## 2. CE QUI EST RÉELLEMENT FONCTIONNEL

### ✅ Sécurité
- `ChifaWriteGuard` bloque les écritures en mode ReadOnly
- `ChifaIntegrationModeProvider` détecte correctement les 3 modes
- DI wiring : mode ReadOnly → tous les services sont `FakeChifaIntegrationProvider`
- `ChifaWriteBlockedException` correctement levée

### ✅ Mapping EF Core
- 6 entités mappées aux 6 bonnes tables
- Clés primaires correctes (sauf bordereau — voir section 4)
- Clés composites correctes (detail_fact : NumFact+NumEnr+Ppa)
- Longueurs MaxLength correctes
- Précisions décimales correctes
- Aucune propriété fantôme

### ✅ Validation
- `ChifaInvoiceValidator` : NumFact ≤8, NumAssure ≤12, NumEnr ≤5, Qte 1-999, PPA >0
- `ChifaBordereauValidator` : NumBord ≤6, CodeCentre requis, ≥1 facture
- Default values correctement appliqués

### ✅ Mapping domain → CHIFA
- `ChifaInvoiceMapper` : MapToChifaRequest, MapNumFact, MapNumEnr, CalculateMont*
- `ChifaBordereauMapper` : MapToChifaRequest, MapNumBord
- Troncature/paddage correct (8 chars facture, 5 chars num_enr, 6 chars bordereau)

### ✅ Workflow
- `OneActionWorkflowService` : 5 étapes (Validate → Prepare → Write → CheckVisibility → CheckSigning)
- États gérés : Validated, PreparedForChifa, WrittenToChifa, VisibleInChifa, WaitingForSigning, Failed
- Human action flag correctement positionné
- `ChifaWorkflowStateMachine` gère les transitions

### ✅ Tests
- 493+ tests passent
- Couverture mapping, validation, guard, DI, mappers, validators

---

## 3. CE QUI EST STUB / INCOMPLET

### 🔴 ChifaPostgresInvoiceService — STUB
```csharp
// La transaction s'ouvre mais NE FAIT AUCUN INSERT
using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
// ... calculates montFact ...
await transaction.CommitAsync(cancellationToken); // Commit vide !
```
**Problème :** Aucun `INSERT INTO facture` ni `INSERT INTO detail_fact`. La transaction est un no-op.

### 🔴 ChifaPostgresBordereauService — STUB
```csharp
// Idem — transaction vide
using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken); // Commit vide !
```
**Problème :** Aucun `INSERT INTO bordereau`.

### 🔴 GetNextBordereauNumberAsync — HARDCODÉ
```csharp
var nextNum = 216; // Hardcoded !
return Task.FromResult(nextNum.ToString("D6"));
```
**Problème :** Ne lit PAS le compteur `parametre.next_num_bord`. Risque de collision.

### 🔴 InvoiceExistsInChifaAsync — TOUJOURS FAUX
```csharp
return Task.FromResult(false); // Toujours false !
```
**Problème :** Ne vérifie pas réellement dans la base.

### 🔴 SignBordereauAsync / CloseBordereauAsync — DÉLÉGUÉ
Retournent systématiquement `Success=false` avec message d'erreur. C'est **intentionnel** (sécurité) mais le workflow ne peut pas avancer au-delà.

---

## 4. INCOHÉRENCES DE MAPPING

### 🔴 ChifaBordereau — PK = IdBord (auto-increment) au lieu de NumBord
```csharp
entity.HasKey(e => e.IdBord); // PK = id_bord (bigint auto-increment)
```
**Problème :** Dans la vraie base, `num_bord` est le PK logique (UNIQUE, NOT NULL). `id_bord` est juste un séquenceur auto-increment. EF Core utilise `IdBord` comme tracking ID, ce qui est acceptable mais il faut ajouter un index unique sur `NumBord`.

### 🟡 ChifaParametre — HasNoKey mais requêteable
```csharp
entity.HasNoKey();
```
**Problème :** Une table keyless ne peut pas être modifiée facilement via EF Core (pas de tracking). Pour les UPDATE de compteurs, il faudra du SQL brut.

### 🟡 ChifaFacture.Signature et FactXml — MaxLength(4000) pour XML
```csharp
entity.Property(e => e.Signature).HasMaxLength(4000);
entity.Property(e => e.FactXml).HasMaxLength(4000);
```
**Problème :** Le type PostgreSQL est `xml`, pas `varchar`. EF Core mappera en `string` (OK) mais le MaxLength pourrait tronquer les gros XML. En pratique, les signatures XML dépassent rarement 4000 chars.

---

## 5. DUPLICATIONS

### 🔴 OnModelCreating dupliqué à 100%
`ChifaPostgreSqlContext.OnModelCreating` et `ChifaWriteDbContext.OnModelCreating` sont **identiques** (180 lignes chacun). Toute modification doit être faite deux fois.

**Solution :** Extraire dans `ChifaEntityConfigurations.cs` partagé.

---

## 6. RISQUES IDENTIFIÉS

### Risque de concurrence
- Si BM Pharma et CHIFA-OFFICINE écrivent simultanément, pas de mécanisme de verrouillage
- PostgreSQL default isolation = READ COMMITTED, pas de verrouillage pessimiste
- **Mitigation :** Utiliser des transactions avec `SELECT ... FOR UPDATE` si nécessaire

### Risque de compteur
- `next_num_fact` et `next_num_bord` sont dans `parametre` (table keyless, 1 seule ligne)
- Pas de mécanisme atomique pour incrementer
- **Mitigation :** SQL brut `UPDATE parametre SET next_num_fact = next_num_fact + 1 RETURNING next_num_fact`

### Risque de transaction partielle
- Les stubs actuels ne font aucun INSERT, donc pas de risque actuel
- Après implémentation : attention à l'ordre des INSERT (detail_fact après facture)

### Risque de mauvaise utilisation du compteur
- Si le compteur est lu puis écrit séparément (TOCTOU), risque de double-attribution
- **Solution :** Utiliser `UPDATE ... RETURNING` ou `SELECT nextval()`

---

## 7. DIFFÉRENCES READONLY / TEST / PRODUCTION

| Aspect | ReadOnly | Test | Production |
|--------|----------|------|------------|
| DI wiring | Fake providers | Real PG contexts | Real PG contexts |
| ChifaWriteGuard | Bloque | Autorise | Autorise |
| Contexts EF | Non enregistrés | InMemory (tests) | Npgsql (réel) |
| Signatures | Impossible | Impossible | Impossible |
| Clôture bordereau | Impossible | Impossible | Impossible |
| CNAS transmission | Impossible | Impossible | Impossible |

---

## 8. RÉSUMÉ

| Catégorie | Count |
|-----------|-------|
| Fichiers analysés | ~30 |
| Entités EF Core | 6 |
| Services | 8 |
| Tests existants | ~665 |
| Stubs à implémenter | 3 (InvoiceService, BordereauService, InvoiceExists) |
| Duplications critiques | 1 (OnModelCreating) |
| Incohérences mapping | 2 (Bordereau PK, Parametre keyless) |
| Risques concurrence | 1 (compteur) |

### Priorités d'action

1. **Extraire ChifaEntityConfigurations** (éliminer duplication)
2. **Implémenter ChifaPostgresInvoiceService** (INSERT facture + detail_fact)
3. **Implémenter ChifaPostgresBordereauService** (INSERT bordereau)
4. **Implémenter InvoiceExistsInChifaAsync** (SELECT vérification)
5. **Ajouter IChifaNumberingService** (compteurs atomiques)
6. **Ajouter ChifaWriteResult / ChifaWriteError** (types de retour structurés)
7. **Renforcer les tests négatifs** (FK invalide, médicament inexistant, timeout)
