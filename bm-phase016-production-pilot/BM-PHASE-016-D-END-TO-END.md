# 016-D — End-to-End Pilot Workflow

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Exécuter et documenter le workflow métier complet.

---

## Environnement

| Champ | Valeur |
|-------|--------|
| **Pharmacie pilote** | |
| **Auteur test** | |
| **Date** | |
| **Version BM Pharma** | |
| **CHIFA version** | |

---

## Workflow

```
Vente → Préparation → Validation → Synchronisation → Création facture → CHIFA → Signature → Clôture → Transmission CNAS
```

---

## Étape 1 — Vente

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 1.1 | Ouvrir BM Pharma | Application démarre | ☐ OK / ☐ KO |
| 1.2 | Dashboard affiche les données CHIFA | Stocks, clients, factures visibles | ☐ OK / ☐ KO |
| 1.3 | Naviguer vers "Préparation de vente" | Vue chargée sans erreur | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 2 — Préparation

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 2.1 | Sélectionner un client | Client chargé | ☐ OK / ☐ KO |
| 2.2 | Ajouter des produits à la vente | Produits ajoutés | ☐ OK / ☐ KO |
| 2.3 | Vérifier les prix calculés | Prix corrects | ☐ OK / ☐ KO |
| 2.4 | Vérifier le stock pour chaque produit | Stock affiché et à jour | ☐ OK / ☐ KO |
| 2.5 | Appliquer une remise (si applicable) | Remise calculée | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 3 — Validation

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 3.1 | Vérifier le récapitulatif de la vente | Montants corrects | ☐ OK / ☐ KO |
| 3.2 | Valider la vente | Validation réussie | ☐ OK / ☐ KO |
| 3.3 | Message de confirmation affiché | "Vente validée" | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 4 — Synchronisation

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 4.1 | Synchronisation déclenchée automatiquement | Sync démarrée | ☐ OK / ☐ KO |
| 4.2 | Vérifier statut Dashboard "Sync OK" | Statut vert | ☐ OK / ☐ KO |
| 4.3 | Vérifier logs de synchronisation | Pas d'erreur | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 5 — Création Facture

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 5.1 | Facture créée dans PostgreSQL | Ligne en base | ☐ OK / ☐ KO |
| 5.2 | Numéro de facture généré | Format attendu | ☐ OK / ☐ KO |
| 5.3 | Montant facture correct | Correspond à la vente | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 6 — CHIFA

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 6.1 | Facture transmise à CHIFA | Reçue par CHIFA | ☐ OK / ☐ KO |
| 6.2 | Vérifier dans CHIFA-OFFICINE | Facture visible | ☐ OK / ☐ KO |
| 6.3 | Données CHIFA cohérentes | Correspond à la vente | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 7 — Signature (si disponible)

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 7.1 | Signature électronique demandée | Interface signature affichée | ☐ OK / ☐ KO / ☐ N/A |
| 7.2 | Signature appliquée | Facture signée | ☐ OK / ☐ KO / ☐ N/A |

**Temps :** s  
**Commentaires :**

---

## Étape 8 — Clôture

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 8.1 | Vente clôturée | Statut "Clôturée" | ☐at OK / ☐ KO |
| 8.2 | Aucune modification possible après clôture | Édition bloquée | ☐ OK / ☐ KO |

**Temps :** s  
**Commentaires :**

---

## Étape 9 — Transmission CNAS

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 9.1 | Transmission déclenchée | Envoi CNAS réussi | ☐ OK / ☐ KO / ☐ N/A |
| 9.2 | Accusé réception CNAS reçu | AR positif | ☐at OK / ☐ KO / ☐ N/A |

**Temps :** s  
**Commentaires :**

---

## Résumé Chronologique

| Étape | Temps | Statut |
|-------|-------|--------|
| 1. Vente | s | ☐ OK / ☐ KO |
| 2. Préparation | s | ☐ OK / ☐ KO |
| 3. Validation | s | ☐ OK / ☐ KO |
| 4. Synchronisation | s | ☐ OK / ☐ KO |
| 5. Création facture | s | ☐ OK / ☐ KO |
| 6. CHIFA | s | ☐ OK / ☐ KO |
| 7. Signature | s | ☐ OK / ☐ KO / ☐ N/A |
| 8. Clôture | s | ☐ OK / ☐ KO |
| 9. Transmission CNAS | s | ☐ OK / ☐ KO / ☐ N/A |
| **Total** | **s** | |

---

## Anomalies

| # | Étape | Description | Impact | Criticité |
|---|-------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Critère | Résultat |
|---------|----------|
| Workflow Vente → CNAS complet | ☐ OK / ☐ KO |
| Synchronisation fonctionnelle | ☐at OK / ☐ KO |
| Données cohérentes entre BM Pharma et CHIFA | ☐ OK / ☐ KO |
| Temps de traitement acceptable | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
