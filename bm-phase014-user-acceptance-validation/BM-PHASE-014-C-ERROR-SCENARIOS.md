# 014-C — Error Scenarios

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Objective:** Valider le comportement de l'application pour chaque scénario d'erreur identifié.

---

## Scenario 1 — PostgreSQL Indisponible

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Dashboard : carte PostgreSQL rouge "Déconnecté". Status bar : "CHIFA: Déconnecté". |
| **Détection** | `ChifaHealthCheckService.CheckAllAsync()` → `IsDatabaseConnected = false`. Délai de timeout Npgsql (~15s). |
| **Message utilisateur** | "PostgreSQL indisponible" (ConnectionStatus = "#F44336"). |
| **Comportement attendu** | • Toutes les opérations CHIFA sont bloquées. • Mode ReadOnly protège les écritures. • Le Circuit Breaker s'ouvre après 3 échecs. • Le dashboard reste utilisable (données locales). |
| **Récupération** | • Rétablir la connexion PostgreSQL. • Le Circuit Breaker passe HalfOpen puis Closed après 2 succès. • Le bouton "Actualiser" restaure l'état. |
| **Test référence** | `ChifaDashboardTests.DASH001-DASH005`, `ChifaNegativeScenarioTests` |
| **Code traçable** | `ChifaHealthCheckService.cs`, `ChifaCircuitBreaker.cs`, `ChifaIntegrationFacade.cs` |

---

## Scenario 2 — CHIFA-OFFICINE Fermé

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Dashboard : carte CHIFA rouge "Hors ligne". Toute opération retourne `NotAvailable`. |
| **Détection** | `IChifaIntegrationService.IsChifaAvailableAsync()` → `false`. |
| **Message utilisateur** | "CHIFA hors ligne" (ChifaStatus = "#F44336"). Tentative d'écriture : "CHIFA est indisponible". |
| **Comportement attendu** | • Mode ReadOnly reste actif. • Toute écriture refuse avec `FacadeOperationStatus.NotAvailable`. • Le dashboard affiche l'état dégradé. |
| **Récupération** | • Démarrer CHIFA-OFFICINE. • Actualiser le dashboard. |
| **Test référence** | `PH013_DASH004` — DashboardOverview returns Degraded when CHIFA unavailable |
| **Code traçable** | `ChifaIntegrationServiceStub.cs`, `FakeChifaIntegrationProvider.cs` |

---

## Scenario 3 — SAM (Token) Absent

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Dashboard : carte Token rouge "Non détecté". Panneau "ACTION REQUISE" orange. |
| **Détection** | `IChifaTokenService.GetTokenStatusAsync()` → `isPresent = false`. |
| **Message utilisateur** | "Token professionnel requis : Pour signer les bordereaux, le pharmacien doit insérer son token PKCS#11 professionnel." |
| **Comportement attendu** | • Les opérations de signature sont bloquées. • La préparation des factures reste possible. • Le panneau d'action requise s'affiche avec les instructions. |
| **Récupération** | • Insérer le token PKCS#11. • La détection se fait au prochain refresh (auto 30s ou manuel). |
| **Test référence** | `ChifaDashboardTests.DASH006-DASH007` |
| **Code traçable** | `ChifaTokenServiceStub.cs`, `ChifaDashboardViewModel.cs` (DetermineRequiredAction) |

---

## Scenario 4 — Connexion Perdue Pendant une Opération

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Exception Npgsql/timeout pendant une écriture. L'opération retourne `NotAvailable` ou `WriteFailed`. |
| **Détection** | Catch dans le facade → `_logger.LogError`. `FacadeResult.Status = WriteFailed`. |
| **Message utilisateur** | "La demande a expiré. Vérifiez la connexion réseau." (via `ChifaExceptionMapper.ToUserMessage()`). |
| **Comportement attendu** | • L'opération échoue proprement (pas de corruption). • Le Circuit Breaker enregistre l'échec. • L'audit log enregistre l'erreur. • Le dashboard reflète l'état dégradé. |
| **Récupération** | • Rétablir la connexion. • Le Circuit Breaker se ferme automatiquement après 2 succès. • Réessayer l'opération. |
| **Test référence** | `PH013_EXC001-002`, `ChifaExceptionTests` |
| **Code traçable** | `ChifaExceptionMapper.cs`, `ChifaIntegrationFacade.cs` (tous les catch) |

---

## Scenario 5 — Facture Déjà Existante

| Champ | Valeur |
|-------|--------|
| **Symptômes** | `PrepareInvoiceAsync` retourne `Conflict` ou `ValidationFailed`. |
| **Détection** | `StatusSynchronizer.LoadInvoiceAsync(numFact)` → snapshot non null. Validation : règle V001 bis. |
| **Message utilisateur** | "Conflit : une facture avec ce numéro existe déjà dans CHIFA." |
| **Comportement attendu** | • La création est refusée. • L'utilisateur doit utiliser un autre numéro de facture. • `FacadeOperationStatus.Conflict` retourné. |
| **Récupération** | • Modifier le N° facture. • Re-soumettre. |
| **Test référence** | Non testé explicitement (manque test conflict). |
| **Code traçable** | `ChifaInvoiceValidator.cs` |

---

## Scenario 6 — Médicament Absent

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Validation échoue avec erreur sur la ligne de facture. |
| **Détection** | `ChifaInvoiceValidator` vérifie le médicament dans la base. Code retour validation. |
| **Message utilisateur** | "Le médicament avec le code CIP saisi n'existe pas dans la base CHIFA." |
| **Comportement attendu** | • La ligne est marquée en erreur. • La facture ne peut pas être soumise. • L'utilisateur doit corriger ou supprimer la ligne. |
| **Récupération** | • Corriger le code CIP. • Supprimer la ligne. • Re-valider. |
| **Test référence** | `ChifaInvoiceValidatorTests.cs` |
| **Code traçable** | `ChifaInvoiceValidator.cs` (V006, V007) |

---

## Scenario 7 — Bordereau Inexistant

| Champ | Valeur |
|-------|--------|
| **Symptômes** | `GetBordereauStatusAsync` retourne `NotFound`. Boutons action désactivés. |
| **Détection** | `BordereauStatusService.GetAllBordereaux()` → collection vide ou filtrage. |
| **Message utilisateur** | "Bordereau introuvable. Vérifiez le numéro de bordereau." |
| **Comportement attendu** | • Aucune action possible sur un bordereau inexistant. • L'interface ne bloque pas. L'utilisateur peut créer un nouveau bordereau. |
| **Récupération** | • Créer un nouveau bordereau. • Recharger la liste. |
| **Test référence** | `ChifaBordereauServiceTests.cs`, `ChifaBordereauStatusServiceTests.cs` |
| **Code traçable** | `ChifaBordereauStatusViewModel.cs`, `BordereauStatusService.cs` |

---

## Scenario 8 — Rollback Impossible

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Une écriture EF Core réussit (`SaveChangesAsync`) mais la lecture CHIFA échoue. État `RollbackRequired`. |
| **Détection** | Workflow state machine → `ChifaWorkflowState.RollbackRequired`. |
| **Message utilisateur** | "L'écriture a été effectuée mais la vérification a échoué. Contactez le support. État : Rollback requis." |
| **Comportement attendu** | • L'audit log enregistre l'erreur. • Le state machine verrouille l'état en `RollbackRequired`. • Aucune autre opération possible sur cette facture sans intervention manuelle. |
| **Récupération** | • Intervention manuelle sur PostgreSQL (supprimer la facture si nécessaire). • Support BM Pharma requis. |
| **Test référence** | `ChifaWorkflowStateMachineTests.cs` (transitions RollbackRequired), `BM-PHASE-012-C-EXCEPTION-AUDIT.md` |
| **Code traçable** | `ChifaWorkflowStateMachine.cs`, `ChifaIntegrationFacade.cs` (ExecuteFullWorkflowAsync) |

---

## Scenario 9 — Circuit Breaker Ouvert

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Dashboard : Circuit Breaker rouge "Open". Toutes les opérations sont refusées immédiatement (pas de tentative réseau). |
| **Détection** | `ChifaCircuitBreaker.GetState("ChifaIntegration")` → `CircuitState.Open`. Seuil : 3 échecs consécutifs. Timeout : 30s avant HalfOpen. |
| **Message utilisateur** | "Service temporairement indisponible (Circuit Breaker ouvert). Réessayez dans quelques instants." |
| **Comportement attendu** | • Toutes les opérations retournent `NotAvailable` sans appel réseau. • Le dashboard affiche "Open" en rouge. • Après 30s, passe en HalfOpen. • 2 succès consécutifs ferment le circuit. |
| **Récupération** | • Attendre le délai (30s). • Le circuit se ferme automatiquement. • Actualiser le dashboard. |
| **Test référence** | `MonitoringTests.cs`, `ChifaCircuitBreaker.cs` (tests unitaires complets) |
| **Code traçable** | `ChifaCircuitBreaker.cs`, `ChifaMonitoringService.cs`, `ChifaDashboardViewModel.cs` |

---

## Scenario 10 — Timeout Réseau

| Champ | Valeur |
|-------|--------|
| **Symptômes** | Opération bloquée plus longtemps que prévu. `TaskCanceledException` levée. |
| **Détection** | Catch dans le facade → `_logger.LogWarning`. Exception mapper → code "TIMEOUT". |
| **Message utilisateur** | "La demande a expiré. Vérifiez la connexion CHIFA-OFFICINE." |
| **Comportement attendu** | • L'opération est annulée. • Aucune donnée corrompue. • Le Circuit Breaker enregistre l'échec. |
| **Récupération** | • Vérifier la connexion réseau. • Réessayer l'opération. |
| **Test référence** | `PH013_EXC002`, `PH013_EXC003` |
| **Code traçable** | `ChifaExceptionMapper.cs` |

---

## Matrice de Gravité

| # | Scénario | Gravité | Probabilité | Détection | Récupération |
|---|----------|---------|-------------|-----------|--------------|
| 1 | PostgreSQL indisponible | **Élevée** | Moyenne | Immédiate | Automatique (CB) |
| 2 | CHIFA fermé | **Élevée** | Faible | Immédiate | Manuelle |
| 3 | SAM absent | **Moyenne** | Élevée | Immédiate | Manuelle |
| 4 | Connexion perdue | **Élevée** | Faible | Temps réel | Automatique (CB) |
| 5 | Facture existante | **Basse** | Faible | Validation | Manuelle |
| 6 | Médicament absent | **Basse** | Moyenne | Validation | Manuelle |
| 7 | Bordereau inexistant | **Basse** | Faible | Requête | Manuelle |
| 8 | Rollback impossible | **Critique** | Très faible | Post-opération | Support requis |
| 9 | Circuit Breaker open | **Moyenne** | Faible | Immédiate | Automatique (30s) |
| 10 | Timeout réseau | **Moyenne** | Faible | Temps réel | Automatique (CB) |

## Conclusion

Tous les scénarios d'erreur sont **détectés** et **gérés**. Le seul scénario critique (rollback impossible) a une probabilité très faible et nécessite une intervention manuelle documentée. Aucun scénario ne provoque de perte de données ou de corruption non récupérable.
