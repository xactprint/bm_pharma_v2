# BM-PHASE-008-A — MATRICE DU WORKFLOW CHIFA

**Document:** 008-A-WORKFLOW-MATRIX
**Date:** 2026-07-28

---

## Workflow complet reconstruit

```mermaid
flowchart LR
    VENTE[Vente<br/>BM Pharma] --> FACTURE[Création facture<br/>INSERT facture]
    FACTURE --> BORDEREAU[Création bordereau<br/>INSERT bordereau<br/>+ UPDATE facture.num_bord]
    BORDEREAU --> VISU[Visualisation<br/>FBordereau]
    VISU --> SIGNATURE[Signature token<br/>Identiv uTrust 3512]
    SIGNATURE --> CLOTURE[Clôture<br/>cloturerbord()]
    CLOTURE --> TRANSMISSION[Transmission CNAS<br/>FTP 41.111.149.250]
    
    style VENTE fill:#4CAF50,color:#fff
    style FACTURE fill:#4CAF50,color:#fff
    style BORDEREAU fill:#FF9800,color:#fff
    style VISU fill:#FF9800,color:#fff
    style SIGNATURE fill:#f44336,color:#fff
    style CLOTURE fill:#f44336,color:#fff
    style TRANSMISSION fill:#f44336,color:#fff
```

| Couleur | Signification |
|---------|---------------|
| 🟢 Vert | ✅ Prouvé / BM Pharma peut le faire |
| 🟠 Orange | 🟡 À tester / partiellement prouvé |
| 🔴 Rouge | 🔴 Bloqué / Nécessite CHIFA-OFFICINE |

---

## Matrice détaillée

### Étape 1 : Création facture

| Sous-étape | Qui | Statut |
|------------|-----|--------|
| Insertion `facture` | BM Pharma (EF Core) | ✅ Phase 007-G |
| Insertion `detail_fact` | BM Pharma (EF Core) | ✅ Phase 007-G |
| Transaction ReadCommitted | PostgreSQL | ✅ Phase 007-G |
| Audit | BM Pharma | ✅ Phase 007-G |
| `Ts` = `false` (bug) | BM Pharma | 🔴 Bug documenté |

### Étape 2 : Création bordereau

| Sous-étape | Qui | Statut | Notes |
|------------|-----|--------|-------|
| `GetNextBordereauNumberAsync` | BM Pharma | 🟡 Non intégré au service | Appelé séparément |
| `INSERT INTO bordereau` | BM Pharma | 🟡 Jamais testé sur PG réel | Testé InMemory |
| `UPDATE facture SET num_bord` | BM Pharma | 🟡 Jamais testé sur PG réel | FK update |

### Étape 3 : Visualisation

| Sous-étape | Qui | Statut | Notes |
|------------|-----|--------|-------|
| Consultation Facture | CHIFA-OFFICINE | 🟡 DÉDUIT | TST001 vu Phase 005 |
| Visualiser Bordereau | CHIFA-OFFICINE | ❓ INCONNU | Théorie `detail_bord` |

### Étape 4 : Signature

| Sous-étape | Qui | Statut | Notes |
|------------|-----|--------|-------|
| Connexion token | CHIFA-OFFICINE | ✅ ARCHITECTURE | Identiv uTrust 3512 |
| Saisie PIN | Utilisateur | ✅ ARCHITECTURE | Pinpad hardware |
| PKCS#7 sign | p7sign.dll | ✅ ARCHITECTURE | Native DLL |
| Stockage signature | PostgreSQL | 🔴 Impossible sans CHIFA | `signature` XML |
| Détection signature | BM Pharma | 🟡 Possible | SELECT signature IS NOT NULL |

### Étape 5 : Clôture

| Sous-étape | Qui | Statut | Notes |
|------------|-----|--------|-------|
| `cloturerbord()` | PostgreSQL | 🔴 Impossible sans signature | `num > 0` exige signature |
| Création dossier | PG_Program.exe | 🔴 Impossible sans cloture | |
| Écriture .P7M | PG_Program.exe | 🔴 Impossible sans signature | |
| Écriture .xml | PG_Program.exe | 🔴 Impossible sans signature | |

### Étape 6 : Transmission CNAS

| Sous-étape | Qui | Statut | Notes |
|------------|-----|--------|-------|
| Connexion FTP 41.111.149.250 | CHIFA-OFFICINE | 🔴 Impossible sans cloture | |
| Upload fichiers | CHIFA-OFFICINE | 🔴 Impossible sans cloture | |
| Mise à jour `date_depot_ftp` | CHIFA-OFFICINE | 🔴 Impossible sans cloture | |
| Impression accusé | CHIFA-OFFICINE | 🔴 Impossible sans cloture | |

---

## États du workflow BM Pharma

### ChifaWorkflowStateMachine

```
Draft → Validated → PreparedForChifa → WrittenToChifa → VisibleInChifa
       → Signed → BordereauAssigned → BordereauClosed → Transmitted
       + Failed, Rejected, RollbackRequired, Cancelled
```

### État réel par rapport à CHIFA

| État BM | Correspond CHIFA | Preuve |
|---------|------------------|--------|
| Draft | - (pas encore dans CHIFA) | ✅ Code |
| Validated | - (validation locale) | ✅ Code |
| PreparedForChifa | - (préparation) | ✅ Code |
| WrittenToChifa | Ligne dans `facture` | ✅ Phase 007-G |
| VisibleInChifa | Visible dans Consultation Facture | 🟡 Phase 005 (TST001) |
| Signed | `facture.signature <> ''` | 🔴 Impossible |
| BordereauAssigned | `facture.num_bord = X` | 🟡 Jamais testé |
| BordereauClosed | `bordereau.etat = '1'` | 🔴 Impossible |
| Transmitted | `bordereau.date_depot_ftp` | 🔴 Impossible |

---

## Responsabilités

| Acteur | Responsabilités |
|--------|----------------|
| **BM Pharma** | Création facture + detail_fact → EF Core → PostgreSQL |
| **BM Pharma** | Création bordereau + association facture (V2) |
| **CHIFA-OFFICINE** | Signature → p7sign.dll → Identiv uTrust 3512 |
| **CHIFA-OFFICINE** | Clôture → cloturerbord() → PG_Program |
| **CHIFA-OFFICINE** | Transmission → FTP → CNAS |
| **Utilisateur** | Insertion carte + PIN |

---

## Conclusion BM Pharma : limites du périmètre

```
BM Pharma peut :
✅ Créer factures dans CHIFA_OFFICINE
✅ Créer bordereaux dans CHIFA_OFFICINE
✅ Lier factures à bordereaux
✅ Auditer l'état CHIFA (SELECT)
✅ Détecter signature (SELECT signature IS NOT NULL)

BM Pharma ne peut PAS (sans CHIFA-OFFICINE) :
🔴 Signer avec le token professionnel
🔴 Exécuter cloturerbord()
🔴 Transmettre à CNAS
```
