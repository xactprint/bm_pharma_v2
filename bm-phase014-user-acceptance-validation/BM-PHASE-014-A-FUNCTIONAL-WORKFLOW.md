# 014-A — Functional Workflow Validation

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Objective:** Validate pas à pas le workflow réel de bout en bout.

---

## Workflow Overview

```
Vente BM Pharma → Préparation CHIFA → Écriture facture → Lecture CHIFA
    → Traitement CHIFA → Création bordereau → Signature → Clôture
    → Transmission CNAS
```

---

## Step 1 — Vente

| Champ | Valeur |
|-------|--------|
| **Entrées** | Panier client (produits, quantités, prix) dans BM Pharma (SQLite) |
| **Sorties** | Facture BM Pharma stockée localement. Aucune interaction CHIFA. |
| **Préconditions** | BM Pharma démarré, PostgreSQL accessible (ReadOnly suffit) |
| **Postconditions** | Facture BM présente dans `Invoice`, `InvoiceLine` (SQLite) |
| **Responsable** | Pharmacien |
| **Application utilisée** | BM Pharma — module Vente (interface standard) |
| **Risques** | Perte de connexion SQLite (local, improbable). Erreur de caisse. |
| **Validation** | ✅ Fonctionnel depuis Phase 001. Testé via `InvoiceTests.cs` (2 tests). |
| **Référence** | `src/BMPharma.Persistence.SQLite/`, `src/BMPharma.Domain/Entities/Invoice.cs` |

---

## Step 2 — Préparation BM Pharma (CHIFA)

| Champ | Valeur |
|-------|--------|
| **Entrées** | N° facture BM, N° assurance, Code centre (11600), Date soin, Lignes de facture (CIP, quantité, PU) |
| **Sorties** | Objet `ChifaInvoiceRequest` validé + aperçu montants. État `Validated`. |
| **Préconditions** | Facture BM créée (Step 1). CHIFA accessible (ou ReadOnly simulé). |
| **Postconditions** | Requête validée. Workflow state machine passe `Draft → Validated`. |
| **Responsable** | Pharmacien |
| **Application utilisée** | BM Pharma — vue "Préparation facture CHIFA" |
| **Risques** | Validation échoue (CIP inconnu, montant nul). Mode ReadOnly bloque écriture. |
| **Validation** | ✅ `ChifaInvoiceValidator.cs` (9 règles). Testé `ChifaInvoiceValidatorTests.cs` (5). |
| **Référence** | `ChifaInvoicePreparationViewModel.cs`, `ChifaInvoiceValidator.cs` |

### Règles de validation (ChifaInvoiceValidator)

| Code | Règle | Message |
|------|-------|---------|
| V001 | N° facture requis | "Le numéro de facture est requis." |
| V002 | N° assurance requis | "Le numéro d'assurance est requis." |
| V003 | Code centre valide | "Le code centre doit être un nombre positif." |
| V004 | Date soin valide | "La date de soin est requise." |
| V005 | Au moins une ligne | "Au moins une ligne de facture est requise." |
| V006 | Quantité positive | "La quantité doit être supérieure à 0." |
| V007 | Prix unitaire positif | "Le prix unitaire doit être supérieur à 0." |
| V008 | N° enregistrement requis | "Le numéro d'enregistrement est requis." |
| V009 | Montant total cohérent | "Le montant total calculé est incohérent." |

---

## Step 3 — Écriture Facture

| Champ | Valeur |
|-------|--------|
| **Entrées** | `ChifaInvoiceRequest` validé |
| **Sorties** | `FacadeResult<ChifaInvoiceResult>` avec état `WrittenToChifa` ou `PreparedForChifa` |
| **Préconditions** | Mode Test ou Production. `ChifaWriteGuard` authorise. |
| **Postconditions** | Ligne écrite dans `facture` + `detail_fact` (PostgreSQL CHIFA_OFFICINE). Audit log créé. |
| **Responsable** | BM Pharma (automatique) |
| **Application utilisée** | BM Pharma → `ChifaIntegrationFacade.CreateInvoiceAsync()` |
| **Risques** | PG 9.3.4 compatibilité. Timeout Npgsql. `DateTime Kind` mismatch. |
| **Validation** | ✅ Testé TST002 (Phase 007-G) : 877ms, écriture réelle réussie. |
| **Référence** | `BM-PHASE-007-G-REAL-WRITE-REPORT.md`, `ChifaPostgresInvoiceService.cs` |

### Mappings EF Core vérifiés

| Colonne PG | Propriété C# | Statut |
|------------|-------------|--------|
| `num_fact` (varchar(8)) | `NumFact` | ✅ |
| `num_assure` (varchar(12)) | `NumAssure` | ✅ |
| `date_fact` (timestamp) | `DateFact` | ✅ (Kind = Unspecified) |
| `date_soin` (date) | `DateSoin` | ✅ |
| `mont_fact` (numeric) | `MontFact` | ✅ |
| `code_centre` (int4) | `CodeCentre` | ✅ |
| `fact_xml` (xml) | `FactXml` | ✅ (ignoré si vide) |
| `signature` (xml) | `Signature` | ✅ (ignoré si vide) |

---

## Step 4 — Lecture CHIFA

| Champ | Valeur |
|-------|--------|
| **Entrées** | `num_fact` (string 8 caractères) |
| **Sorties** | `ChifaInvoiceStatusSnapshot` ou `null` |
| **Préconditions** | Facture écrite (Step 3). CHIFA-OFFICINE disponible. |
| **Postconditions** | Statut lu depuis PostgreSQL (`ChifaPostgreSqlContext`, read-only). |
| **Responsable** | BM Pharma (automatique) |
| **Application utilisée** | BM Pharma → `StatusSynchronizer.LoadInvoiceAsync()` |
| **Risques** | PG 9.3.4. Délai de propagation. Facture non trouvée. |
| **Validation** | ✅ Testé via `StatusEngineTests.cs`, `ChifaIntegrationFacadeTests.cs` |
| **Référence** | `StatusSynchronizer.cs`, `ChifaPostgreSqlContext.cs` |

---

## Step 5 — Traitement CHIFA

| Champ | Valeur |
|-------|--------|
| **Entrées** | Facture lue (`ChifaInvoiceStatusSnapshot`) |
| **Sorties** | Mise à jour du statut métier (`BusinessStatus`) et visibilité |
| **Préconditions** | Facture présente dans CHIFA-OFFICINE |
| **Postconditions** | `Visibility` mis à jour. `BusinessStatus` évalué via `StatusEngine`. |
| **Responsable** | Automatic (StatutEngine) ou manuel (pharmacien dans CHIFA-OFFICINE) |
| **Application utilisée** | BM Pharma → `StatusEngine.EvaluateAsync()` |
| **Risques** | CHIFA-OFFICINE modifie l'état hors bande (création bordereau, signature) |
| **Validation** | ✅ `StatusEngineTests.cs` (20+ tests). |
| **Référence** | `StatusEngine.cs`, `StatusEngineModels.cs` |

---

## Step 6 — Création Bordereau

| Champ | Valeur |
|-------|--------|
| **Entrées** | `numBord`, `codeCentre`, `List<string> invoiceNumbers` |
| **Sorties** | `BordereauWorkflowSummary` avec état `Created` ou `InvoicesAttached` |
| **Préconditions** | Mode Test/Production. Factures préparées et visibles. |
| **Postconditions** | Bordereau créé dans `bordereau`. Factures liées. |
| **Responsable** | Pharmacien (via BM Pharma ou CHIFA-OFFICINE) |
| **Application utilisée** | BM Pharma — vue "Statut Bordereau" ou CHIFA-OFFICINE |
| **Risques** | Facture déjà dans un bordereau. Numéro bordereau en double. |
| **Validation** | ✅ `BordereauWorkflowStateMachineTests.cs` (19 tests). |
| **Référence** | `ChifaBordereauStatusViewModel.cs`, `BordereauStatusService.cs` |

---

## Step 7 — Signature

| Champ | Valeur |
|-------|--------|
| **Entrées** | `numBord` |
| **Sorties** | Bordereau signé (état `Closed` ou `AwaitingTransmission`) |
| **Préconditions** | Token PKCS#11 inséré. CHIFA-OFFICINE disponible. Mode non ReadOnly. |
| **Postconditions** | Bordereau signé. Factures marquées signées. |
| **Responsable** | Pharmacien |
| **Application utilisée** | CHIFA-OFFICINE (signature réelle) ou BM Pharma (orchestration) |
| **Risques** | Token absent. Signature échoue. TIMEOUT CHIFA-OFFICINE. |
| **Validation** | ✅ `ChifaSigningServiceTests.cs`, mécanisme `SigningRequired` |
| **Référence** | `BM-PHASE-012-B-WORKFLOW-AUDIT.md`, `BordereauWorkflowState.cs` |

---

## Step 8 — Clôture

| Champ | Valeur |
|-------|--------|
| **Entrées** | Bordereau signé |
| **Sorties** | Bordereau clôturé (état `Closed`) |
| **Préconditions** | Bordereau signé. Toutes les factures signées. |
| **Postconditions** | Bordereau prêt pour transmission. |
| **Responsable** | Pharmacien |
| **Application utilisée** | BM Pharma ou CHIFA-OFFICINE |
| **Risques** | Factures non signées. Conflit de clôture. |
| **Validation** | ✅ `BordereauWorkflowStateMachineTests.cs` |
| **Référence** | `BordereauStatusService.cs` |

---

## Step 9 — Transmission CNAS

| Champ | Valeur |
|-------|--------|
| **Entrées** | Bordereau clôturé |
| **Sorties** | Bordereau transmis (état `Transmitted`) |
| **Préconditions** | CNAS accessible (via CHIFA-OFFICINE). Bordereau clôturé. |
| **Postconditions** | Bordereau transmis à la CNAS. |
| **Responsable** | Pharmacien (via CHIFA-OFFICINE) |
| **Application utilisée** | CHIFA-OFFICINE (CNAS transmission est une fonctionnalité CHIFA native) |
| **Risques** | CNAS indisponible. Bordereau rejeté. |
| **Validation** | ⚠ Non testé automatiquement. CNAS transmission hors scope BM Pharma. |
| **Référence** | `ICnasService.cs` (stub). Transmission réelle via CHIFA-OFFICINE uniquement. |

---

## Summary — Workflow State Machine Coverage

| État | Classe | Tests | Valide |
|------|--------|-------|--------|
| `Draft` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `Validated` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `PreparedForChifa` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `WrittenToChifa` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `VisibleInChifa` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `Signed` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `BordereauAssigned` | `BordereauWorkflowStateMachine` | ✅ | ✅ |
| `BordereauClosed` | `BordereauWorkflowStateMachine` | ✅ | ✅ |
| `Transmitted` | `BordereauWorkflowStateMachine` | ✅ | ✅ |
| `Failed`, `Rejected` | Both state machines | ✅ | ✅ |
| `RollbackRequired` | `ChifaWorkflowStateMachine` | ✅ | ✅ |
| `Cancelled` | `ChifaWorkflowStateMachine` | ✅ | ✅ |

---

## Constats

1. **Étapes 1–5** (Vente → Validation → Écriture → Lecture → Traitement) sont entièrement automatisées et validées.
2. **Étapes 6–8** (Bordereau → Signature → Clôture) sont orchestrées par BM Pharma mais la signature réelle dépend de CHIFA-OFFICINE.
3. **Étape 9** (Transmission CNAS) est **hors scope BM Pharma** — déléguée à CHIFA-OFFICINE.
4. Aucun workflow bloquant identifié.
