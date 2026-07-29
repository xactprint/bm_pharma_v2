# 014-B — User Acceptance Checklist

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Audience:** Pharmacien testeur (pilote)  
**Instructions:** Cocher chaque case après validation fonctionnelle.

---

## 1. Démarrage et Configuration

- [ ] **1.1** L'application BM Pharma se lance sans erreur
- [ ] **1.2** Le mode d'intégration est affiché correctement (ReadOnly/Test/Production)
- [ ] **1.3** La connexion PostgreSQL est détectée (status bar: vert)
- [ ] **1.4** La connexion CHIFA-OFFICINE est détectée (status bar: vert)
- [ ] **1.5** Le token PKCS#11 est détecté s'il est présent
- [ ] **1.6** Le mode ReadOnly est clairement indiqué (bandeau orange)
- [ ] **1.7** Le message "Prêt" s'affiche dans la status bar

**Référence:** `ChifaDashboardView.xaml`, `MainWindow.xaml`, `BM-PHASE-013-UI-INTEGRATION.md`

---

## 2. Tableau de Bord

- [ ] **2.1** Les 6 cartes de statut s'affichent (PostgreSQL, CHIFA, Token, Signature, Factures, Bordereaux)
- [ ] **2.2** Les couleurs des cartes sont cohérentes (vert=OK, orange=dégradé, rouge=erreur)
- [ ] **2.3** Le nombre de factures du jour est affiché (valeur réelle depuis PostgreSQL)
- [ ] **2.4** Le nombre de bordereaux en attente est affiché
- [ ] **2.5** Les 8 étapes du workflow sont visibles avec leur statut
- [ ] **2.6** L'état du Circuit Breaker est affiché (Fermé/Open/HalfOpen)
- [ ] **2.7** Les métriques de monitoring sont visibles (total, succès, échecs, temps moyen)
- [ ] **2.8** L'ID de corrélation de la dernière opération est visible
- [ ] **2.9** Le résultat de la dernière synchronisation est affiché
- [ ] **2.10** Le bouton "Actualiser" rafraîchit toutes les données
- [ ] **2.11** L'auto-rafraîchissement (30s) fonctionne
- [ ] **2.12** Les panneaux d'action requise s'affichent correctement (token manquant, signature requise)

**Référence:** `ChifaDashboardViewModel.cs`, `ChifaDashboardView.xaml`

---

## 3. Préparation Facture

- [ ] **3.1** Le formulaire de saisie est accessible
- [ ] **3.2** Les champs N° facture, N° assurance, Code centre, Date soin, Patient sont modifiables
- [ ] **3.3** L'ajout de lignes de facture fonctionne (bouton "+ Ajouter ligne")
- [ ] **3.4** La suppression de lignes fonctionne
- [ ] **3.5** Les montants sont recalculés automatiquement (Total, 70%, Reste à payer)
- [ ] **3.6** La validation des champs s'affiche en cas d'erreur
- [ ] **3.7** L'aperçu avant soumission fonctionne (bouton "Aperçu")
- [ ] **3.8** L'aperçu affiche : nombre de lignes, montants, mode, règles de validation
- [ ] **3.9** Le bouton "Valider" exécute la validation CHIFA
- [ ] **3.10** Le bouton "Préparer et soumettre" exécute le workflow complet
- [ ] **3.11** L'ID de corrélation est affiché après chaque opération
- [ ] **3.12** Le journal d'audit se met à jour après chaque opération
- [ ] **3.13** Le bouton "Vider" réinitialise le formulaire
- [ ] **3.14** En mode ReadOnly, un message clair indique que l'opération est simulée
- [ ] **3.15** Les messages d'erreur sont compréhensibles (français, non techniques)

**Référence:** `ChifaInvoicePreparationViewModel.cs`, `ChifaInvoicePreparationView.xaml`

---

## 4. Suivi Bordereau

- [ ] **4.1** La liste des bordereaux se charge
- [ ] **4.2** Le détail d'un bordereau s'affiche au clic (N° bordereau, état, factures, montant)
- [ ] **4.3** Les boutons d'action (Valider, Signer, Clôturer, Transmettre) sont présents
- [ ] **4.4** Les boutons d'action sont désactivés si aucune action requise
- [ ] **4.5** La validation d'un bordereau fonctionne
- [ ] **4.6** La signature d'un bordereau fonctionne (ou message si token absent)
- [ ] **4.7** La clôture d'un bordereau fonctionne
- [ ] **4.8** La transmission CNAS fonctionne (ou déléguée à CHIFA-OFFICINE)
- [ ] **4.9** Les erreurs de validation sont affichées clairement
- [ ] **4.10** Le journal d'audit du bordereau est visible
- [ ] **4.11** Le nombre d'alertes (bordereaux nécessitant action) est affiché

**Référence:** `ChifaBordereauStatusViewModel.cs`, `ChifaBordereauStatusView.xaml`

---

## 5. Synchronisation

- [ ] **5.1** La synchronisation des factures peut être déclenchée
- [ ] **5.2** La synchronisation des bordereaux peut être déclenchée
- [ ] **5.3** Le résultat de la synchronisation est affiché (nombre de factures/bordereaux trouvés)
- [ ] **5.4** Les erreurs de synchronisation sont signalées
- [ ] **5.5** Le tableau de bord se met à jour après synchronisation

**Référence:** `InvoiceSynchronizer.cs`, `BordereauSynchronizer.cs`, `ChifaDashboardViewModel.cs`

---

## 6. Monitoring et Diagnostics

- [ ] **6.1** L'état du Circuit Breaker est visible
- [ ] **6.2** Les métriques (total opérations, succès, échecs, temps moyen) sont visibles
- [ ] **6.3** L'ID de corrélation est traçable pour chaque opération
- [ ] **6.4** Le journal d'audit est accessible pour les factures
- [ ] **6.5** Le journal d'audit est accessible pour les bordereaux
- [ ] **6.6** La dernière opération est horodatée
- [ ] **6.7** Les erreurs passées sont visibles

**Référence:** `ChifaMonitoringService.cs`, `ChifaDashboardView.xaml`

---

## 7. Gestion des Erreurs

- [ ] **7.1** Si PostgreSQL est arrêté : message clair "PostgreSQL indisponible"
- [ ] **7.2** Si CHIFA-OFFICINE est fermé : message clair "CHIFA hors ligne"
- [ ] **7.3** Si le token est absent en production : message "Token professionnel requis"
- [ ] **7.4** Si la connexion est perdue pendant une opération : message expliquant l'erreur
- [ ] **7.5** Si une facture existe déjà : message de conflit
- [ ] **7.6** Si un médicament n'existe pas : message de validation
- [ ] **7.7** Si un bordereau n'existe pas : message "Bordereau introuvable"
- [ ] **7.8** Si le Circuit Breaker est ouvert : message "Service temporairement indisponible"
- [ ] **7.9** Les messages d'erreur sont en français
- [ ] **7.10** Les messages d'erreur ne contiennent pas de stack trace

**Référence:** `ChifaExceptionMapper.cs`, `BM-PHASE-014-C-ERROR-SCENARIOS.md`

---

## 8. Performance (Perception Utilisateur)

- [ ] **8.1** Le tableau de bord se charge en moins de 3 secondes
- [ ] **8.2** La validation d'une facture prend moins de 2 secondes
- [ ] **8.3** L'aperçu d'une facture est instantané
- [ ] **8.4** La soumission d'une facture prend moins de 5 secondes
- [ ] **8.5** La synchronisation prend moins de 10 secondes
- [ ] **8.6** Le rafraîchissement du tableau de bord prend moins de 3 secondes
- [ ] **8.7** L'interface reste réactive pendant les opérations
- [ ] **8.8** Aucun freeze ou blocage de l'interface utilisateur

**Référence:** `BM-PHASE-014-E-PERFORMANCE-VALIDATION.md`

---

## 9. Scénario Complet (Workflow de bout en bout)

- [ ] **9.1** Créer une vente dans BM Pharma
- [ ] **9.2** Préparer la facture CHIFA avec 3 lignes de produit
- [ ] **9.3** Valider la facture
- [ ] **9.4** Afficher l'aperçu et vérifier les montants
- [ ] **9.5** Soumettre la facture (ou simulation en ReadOnly)
- [ ] **9.6** Vérifier le statut dans le tableau de bord
- [ ] **9.7** Vérifier le journal d'audit
- [ ] **9.8** Consulter la liste des bordereaux
- [ ] **9.9** Rafraîchir le tableau de bord et vérifier les métriques

---

## Résultat

| Section | Cases cochées | Total | Score |
|---------|--------------|-------|-------|
| 1. Démarrage | __ | 7 | __% |
| 2. Tableau de bord | __ | 12 | __% |
| 3. Préparation facture | __ | 15 | __% |
| 4. Suivi bordereau | __ | 11 | __% |
| 5. Synchronisation | __ | 5 | __% |
| 6. Monitoring | __ | 7 | __% |
| 7. Gestion erreurs | __ | 10 | __% |
| 8. Performance | __ | 8 | __% |
| 9. Scénario complet | __ | 9 | __% |
| **TOTAL** | **__** | **84** | **__%** |

**Seuil d'acceptation :** ≥ 90% (76/84) avec 0 blocant non résolu.

**Date de validation :** _________________

**Signature du pharmacien testeur :** _________________
