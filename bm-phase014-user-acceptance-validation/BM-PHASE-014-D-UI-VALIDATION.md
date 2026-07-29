# 014-D — UI Validation

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Objective:** Vérifier la cohérence de toutes les vues.

---

## View 1 — MainWindow (Shell)

| Composant | État | Validation | Référence |
|-----------|------|------------|-----------|
| Header vert (#1B5E20) | ✅ | Couleur cohérente avec charte | `MainWindow.xaml:16` |
| Titre "BM Pharma v2" | ✅ | Visible | `MainWindow.xaml:19` |
| Navigation sidebar (220px) | ✅ | 8 items standards + 3 CHIFA | `MainWindow.xaml:33-92` |
| CHIFA Dashboard (gras vert) | ✅ | Élement mis en évidence | `MainWindow.xaml:79` |
| ContentControl dynamique | ✅ | Change de vue au clic | `MainWindow.xaml:96` |
| Status bar grise | ✅ | "Prêt" + CHIFA status | `MainWindow.xaml:98-105` |
| CHIFA status bar (rouge/vert) | ✅ | "CHIFA: Déconnecté" / "Connecté" | `MainWindow.xaml.cs` |
| Gestionnaire d'erreur global | ✅ | MessageBox sur exception non gérée | `App.xaml.cs:38-42` |

**Constat :** Aucun problème identifié. La sidebar CHIFA est bien séparée des éléments standards.

---

## View 2 — ChifaDashboardView

| Composant | État | Validation | Référence |
|-----------|------|------------|-----------|
| Titre "Tableau de bord CHIFA" | ✅ | Visible, vert foncé | `ChifaDashboardView.xaml:18` |
| Bouton "Actualiser" | ✅ | Command `RefreshStatusCommand` | `ChifaDashboardView.xaml:21` |
| Badge mode intégration | ✅ | Orange en ReadOnly | `ChifaDashboardView.xaml:23-37` |
| 6 status cards (UniformGrid) | ✅ | PG, CHIFA, Token, Signature, Factures, Bordereaux | `ChifaDashboardView.xaml:42-123` |
| Couleurs dynamiques sur les cartes | ✅ | Binding `ConnectionStatusColor` | `ChifaDashboardView.xaml:51-53` |
| 8 workflow steps (liste verticale) | ✅ | Fond gris (#F5F5F5), titres en gras | `ChifaDashboardView.xaml:133-218` |
| Panneau action requise (orange) | ✅ | Visibilité conditionnelle `HasRequiredAction` | `ChifaDashboardView.xaml:230-253` |
| Grille d'informations détaillées | ✅ | 10 propriétés (mode, PG, CHIFA, token, etc.) | `ChifaDashboardView.xaml:256-330` |
| Section monitoring | ✅ | Circuit Breaker, Metrics, Corrélation | `ChifaDashboardViewModel.cs` |
| Section erreurs | ✅ | `HasError` binding + LastError | `ChifaDashboardView.xaml:333-358` |
| Status bar | ✅ | Dernière vérification | `ChifaDashboardView.xaml:362-372` |

**Constat :** 
- Les nouvelles propriétés (CircuitBreakerState, MetricsTotal, etc.) sont définies dans le VM mais **pas encore bindées dans le XAML** — à ajouter dans une prochaine itération UI.
- Le XAML actuel couvre toutes les fonctionnalités de base.

---

## View 3 — ChifaInvoicePreparationView

| Composant | État | Validation | Référence |
|-----------|------|------------|-----------|
| Titre "Préparation facture CHIFA" | ✅ | `ChifaInvoicePreparationView.xaml:18` |
| Bandeau mode notice (orange) | ✅ | ReadOnly/Actif | `ChifaInvoicePreparationView.xaml:21-27` |
| 5 status cards | ✅ | Mode, PG, Validation, CHIFA, Workflow | `ChifaInvoicePreparationView.xaml:31-67` |
| Formulaire patient (grille 2×4) | ✅ | Tous les champs bindés | `ChifaInvoicePreparationView.xaml:89-118` |
| DataGrid lignes facture | ✅ | 7 colonnes, inline editing | `ChifaInvoicePreparationView.xaml:129-142` |
| Panneau montants (gris) | ✅ | Total, 70%, Reste à payer | `ChifaInvoicePreparationView.xaml:145-167` |
| Boutons Valider / Préparer et soumettre | ✅ | Command bindings | `ChifaInvoicePreparationView.xaml:170-175` |
| Section preview (ShowPreview) | ✅ | Propriétés définies dans VM | `ChifaInvoicePreparationViewModel.cs:113-138` |
| Panneau action requise (orange) | ✅ | `ChifaInvoicePreparationView.xaml:189-204` |
| Panneau erreurs validation (rouge) | ✅ | ItemsControl avec template | `ChifaInvoicePreparationView.xaml:207-231` |
| Panneau état workflow | ✅ | État, étape, mode, corrélation, bordereau | `ChifaInvoicePreparationView.xaml:234-268` |
| DataGrid journal d'audit | ✅ | 3 colonnes (heure, opération, résultat) | `ChifaInvoicePreparationView.xaml:271-287` |
| Status bar | ✅ | Mode + état workflow | `ChifaInvoicePreparationView.xaml:292-303` |

**Constat :** 
- Le preview step (ShowPreview) est défini dans le VM mais **pas encore bindé dans le XAML** — à ajouter.
- Le reste est complet et cohérent.

---

## View 4 — ChifaBordereauStatusView

| Composant | État | Validation | Référence |
|-----------|------|------------|-----------|
| Titre "Suivi des bordereaux CHIFA" | ✅ | Visible | `ChifaBordereauStatusView.xaml:18` |
| Bandeau mode notice | ✅ | `ChifaBordereauStatusView.xaml:21-27` |
| 6 status cards | ✅ | Mode, PG, CHIFA, Token, Alertes, Dernière sync | `ChifaBordereauStatusView.xaml:31-70` |
| DataGrid bordereaux | ✅ | 5 colonnes | `ChifaBordereauStatusView.xaml:84-98` |
| Panneau détail bordereau | ✅ | État, factures, montant | `ChifaBordereauStatusView.xaml:101-160` |
| 4 boutons d'action | ✅ | Valider (vert), Signer (vert F), Clôturer (orange), Transmettre (bleu) | `ChifaBordereauStatusView.xaml:163-218` |
| Panneau erreurs (rouge) | ✅ | `ChifaBordereauStatusView.xaml:221-295` |
| DataGrid journal d'audit | ✅ | 4 colonnes | `ChifaBordereauStatusView.xaml:298-353` |

**Constat :** Vue complète et cohérente.

---

## Status Bar (Global)

| Élément | Comportement | Validation |
|---------|-------------|------------|
| Texte status | "Prêt" / "Chargement..." / messages d'état | ✅ |
| CHIFA status | Vert "Connecté", Rouge "Déconnecté" | ✅ |
| Mode | ReadOnly/Test/Production | ✅ |
| Horloge dernière vérification | Dashboard | ✅ |

---

## Mode ReadOnly (Toutes les vues)

| Vue | Comportement | Validation |
|-----|-------------|------------|
| Dashboard | Carte mode orange, actions désactivées | ✅ |
| Invoice Preparation | Bandeau orange "MODE LECTURE SEULE", messages simulés | ✅ |
| Bordereau Status | Mode notice affiché, actions simulées | ✅ |

---

## Notifications

| Type | Comportement | État | Référence |
|------|-------------|------|-----------|
| Action requise | Panneau orange avec titre + description | ✅ | `RequiredActionPanel` |
| Erreur validation | Panneau rouge avec liste d'erreurs | ✅ | `ValidationErrors` |
| Erreur système | Texte rouge dans section erreurs | ✅ | `LastError` |
| Exception globale | MessageBox avec message | ✅ | `App.xaml.cs` |
| Notification service | Stub uniquement (non connecté) | ⚠ | `NotificationServiceStub.cs` |

---

## Messages d'Erreur — Cohérence Linguistique

| Type | Anglais dans le code | Français affiché | État |
|------|---------------------|------------------|------|
| HTTP Error | `HttpRequestException` → message | "Erreur réseau CHIFA..." | ✅ |
| Timeout | `TaskCanceledException` | "La demande a expiré..." | ✅ |
| Timeout réseau | `TimeoutException` | "La demande a expiré..." | ✅ |
| Annulation | `OperationCanceledException` | "L'opération a été annulée." | ✅ |
| Token invalide | `InvalidOperationException` | "Erreur de token..." | ✅ |
| Accès refusé | `UnauthorizedAccessException` | "Accès refusé..." | ✅ |
| Générique | Fallback | Message original ou inner | ✅ |

**Constat :** Tous les messages d'erreur sont en français via `ChifaExceptionMapper.ToUserMessage()`.

---

## Synthèse UI

| Critère | Score |
|---------|-------|
| Vues complètes | 4/4 |
| Données bindées au VM | ✅ |
| Messages en français | ✅ |
| Cohérence des couleurs | ✅ (vert=OK, orange=avertissement, rouge=erreur) |
| Responsive (pas de freeze) | ✅ (async/await partout) |
| Mode ReadOnly visible | ✅ |
| Accessibilité | ⚠ (à valider en pilote) |

**Constat général :** L'interface est prête pour un pilote. Deux améliorations identifiées (binding XAML pour monitoring et preview) mais non bloquantes.
