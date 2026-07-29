# 016-J — Production Readiness Review

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Comparer les résultats obtenus avec les critères de Phase 015, attribuer un score, lister anomalies et risques.

---

## Référence

| Champ | Valeur |
|-------|--------|
| **Critères de référence** | Phase 015-K (Pilot Acceptance), Phase 015-L (Final Assessment) |
| **Auteur** | |
| **Date** | |

---

## 1. Score par Axe (selon grille 015-L)

### 1.1 Technique (30%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Installation MSI | 10% | /10 | |
| Connexion PostgreSQL | 15% | /15 | |
| Connexion CHIFA | 15% | /15 | |
| Synchronisation | 20% | /20 | |
| Backup | 15% | /15 | |
| Update | 10% | /10 | |
| Diagnostic Center | 15% | /15 | |
| **Sous-total technique** | **100%** | | **/100** |

### 1.2 Fonctionnel (30%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Dashboard (données correctes) | 20% | /20 | |
| Consultation stocks | 15% | /15 | |
| Consultation factures | 15% | /15 | |
| Consultation clients | 15% | /15 | |
| Recherche et filtres | 15% | /15 | |
| Navigation et ergonomie | 10% | /10 | |
| Lisibilité des données | 10% | /10 | |
| **Sous-total fonctionnel** | **100%** | | **/100** |

### 1.3 Performance (20%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Démarrage application | 15% | /15 | |
| Chargement Dashboard | 20% | /20 | |
| Temps réponse CHIFA | 25% | /25 | |
| Stabilité (uptime) | 25% | /25 | |
| Mémoire application | 15% | /15 | |
| **Sous-total performance** | **100%** | | **/100** |

### 1.4 Support (20%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Temps réponse S1 | 20% | /20 | |
| Temps réponse S2 | 15% | /15 | |
| Satisfaction pharmacien | 30% | /30 | |
| Nombre d'incidents S1 | 20% | /20 | |
| Résolution incidents | 15% | /15 | |
| **Sous-total support** | **100%** | | **/100** |

---

## 2. Score Final

```
Score Final = (Technique × 30%) + (Fonctionnel × 30%)
            + (Performance × 20%) + (Support × 20%)
```

| Axe | Score | Pondération | Ponderé |
|-----|-------|-------------|---------|
| Technique | /100 | × 30% | |
| Fonctionnel | /100 | × 30% | |
| Performance | /100 | × 20% | |
| Support | /100 | × 20% | |
| **Score Final** | | **100%** | **/100** |

### Seuils

| Score | Décision |
|-------|----------|
| ≥ 85% | GO Production |
| 70–84% | GO Conditionnel |
| 50–69% | Prolongation Pilote |
| < 50% | NO-GO |

---

## 3. Points Validés

| # | Point | Document de référence |
|---|-------|----------------------|
| | | |
| | | |

---

## 4. Réserves

| # | Réserve | Impact | Plan d'action |
|---|---------|--------|---------------|
| | | | |

---

## 5. Anomalies

| # | Document | Anomalie | Criticité | Statut |
|---|----------|----------|-----------|--------|
| | | | ☐ Bloquante / ☐ Majeure / ☐ Mineure | ☐ Ouvert / ☐ Résolu |
| | | | ☐ Bloquante / ☐ Majeure / ☐ Mineure | ☐ Ouvert / ☐ Résolu |

---

## 6. Risques Résiduels

| # | Risque | Probabilité | Impact | Atténuation |
|---|--------|-------------|--------|-------------|
| | | ☐ Faible / ☐ Moyenne / ☐ Haute | ☐ Faible / ☐ Moyen / ☐ Élevé | |
| | | ☐ Faible / ☐ Moyenne / ☐ Haute | ☐ Faible / ☐ Moyen / ☐ Élevé | |

---

## 7. Comparatif Phase 015 Critères

### C1 — Installation et Configuration

| # | Critère | Résultat |
|---|---------|----------|
| C1.1 | Installation MSI réussie | ☐ OK / ☐ KO |
| C1.2 | Configuration PostgreSQL OK | ☐ OK / ☐ KO |
| C1.3 | Configuration CHIFA OK | ☐ OK / ☐ KO |
| C1.4 | Synchronisation OK | ☐ OK / ☐ KO |
| C1.5 | Backup automatique configuré | ☐ OK / ☐ KO |

### C2 — Fonctionnalités Métier

| # | Critère | Résultat |
|---|---------|----------|
| C2.1 | Dashboard affiche les données CHIFA | ☐ OK / ☐ KO |
| C2.2 | Consultation des stocks | ☐ OK / ☐ KO |
| C2.3 | Consultation des factures | ☐ OK / ☐ KO |
| C2.4 | Consultation des clients | ☐ OK / ☐ KO |
| C2.5 | Recherche fonctionnelle | ☐ OK / ☐ KO |
| C2.6 | Filtres et tris fonctionnent | ☐ OK / ☐ KO |

### C3 — ReadOnly (J+0 à J+7)

| # | Critère | Résultat |
|---|---------|----------|
| C3.1 | Aucune écriture en base CHIFA | ☐ OK / ☐ KO |
| C3.2 | Message ReadOnly visible | ☐ OK / ☐ KO |
| C3.3 | Boutons d'écriture désactivés | ☐ OK / ☐ KO |
| C3.4 | Confirmation pharmacien (J+7) | ☐ OK / ☐ KO |

### C4 — Performance

| # | Critère | Seuil | Résultat |
|---|---------|-------|----------|
| C4.1 | Temps démarrage application | < 10s | |
| C4.2 | Temps chargement Dashboard | < 3s | |
| C4.3 | Temps réponse CHIFA | < 5s | |
| C4.4 | Temps réponse recherche | < 2s | |
| C4.5 | Mémoire application | < 256 MB | |

### C5 — Stabilité

| # | Critère | Seuil | Résultat |
|---|---------|-------|----------|
| C5.1 | Disponibilité application | > 99% | |
| C5.2 | Disponibilité CHIFA | > 99% | |
| C5.3 | Disponibilité PostgreSQL | > 99% | |
| C5.4 | Erreurs non-bloquantes | < 5/semaine | |
| C5.5 | Blocage métier | 0 | |

### C6 — Support

| # | Critère | Résultat |
|---|---------|----------|
| C6.1 | Temps réponse S1 < 15 min | ☐ OK / ☐ KO |
| C6.2 | Temps réponse S2 < 30 min | ☐ OK / ☐ KO |
| C6.3 | Temps réponse S3 < 2h | ☐ OK / ☐ KO |
| C6.4 | Pharmacien satisfait (≥ 4/5) | ☐ OK / ☐ KO |

### C7 — Sauvegarde et Restauration

| # | Critère | Résultat |
|---|---------|----------|
| C7.1 | Backup PostgreSQL OK | ☐ OK / ☐ KO |
| C7.2 | Backup SQLite OK | ☐ OK / ☐ KO |
| C7.3 | Test restauration (J+7) | ☐ OK / ☐ KO |

---

## 8. Synthèse

| Critère | Résultat |
|---------|----------|
| Score final ≥ 85% (GO direct) | ☐ Oui / ☐ Non |
| Score final ≥ 70% (GO conditionnel) | ☐ Oui / ☐ Non |
| Aucune anomalie bloquante | ☐ Oui / ☐ Non |
| Tous les critères Phase 015 validés | ☐ Oui / ☐ Non |
| Pharmacien accepte | ☐ Oui / ☐ Non |

---

## Recommandation pour le Comité

```
Recommandation : GO / GO Conditionnel / Prolongation / NO-GO
Justification :
____________________________________________________________
____________________________________________________________
```
