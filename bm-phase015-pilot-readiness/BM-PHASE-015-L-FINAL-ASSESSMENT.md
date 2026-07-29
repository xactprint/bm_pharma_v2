# 015-L — Final Assessment

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Bilan final du pilote et recommandation pour le déploiement en production.

---

## 1. Assessment Structure

L'évaluation finale combine les résultats des critères d'acceptation (015-K), les métriques de monitoring (015-I), l'historique des incidents (015-J), et la validation métier.

| Axe | Pondération | Source |
|-----|-------------|--------|
| Technique | 30% | 015-B, 015-I |
| Fonctionnel | 30% | 015-K (Questionnaire) |
| Performance | 20% | 015-I, 015-K |
| Support | 20% | 015-J |

---

## 2. Grille d'Évaluation

### 2.1 Technique (30%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Installation MSI | 10% | ☐ Succès / ☐ Échec | /10 |
| Connexion PostgreSQL | 15% | ☐ Stable / ☐ Intermittent / ☐ Instable | /15 |
| Connexion CHIFA | 15% | ☐ Stable / ☐ Intermittent / ☐ Instable | /15 |
| Synchronisation | 20% | ☐ OK / ☐ Partiel / ☐ KO | /20 |
| Backup | 15% | ☐ Automatique / ☐ Manuel / ☐ Absent | /15 |
| Update | 10% | ☐ OK / ☐ Partiel / ☐ KO | /10 |
| Diagnostic Center | 15% | ☐ Tous OK / ☐ Partiel / ☐ KO | /15 |
| **Sous-total technique** | **100%** | | **/100** |

### 2.2 Fonctionnel (30%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Dashboard (données correctes) | 20% | ☐ OK / ☐ Partiel / ☐ KO | /20 |
| Consultation stocks | 15% | ☐ OK / ☐ Partiel / ☐ KO | /15 |
| Consultation factures | 15% | ☐ OK / ☐ Partiel / ☐ KO | /15 |
| Consultation clients | 15% | ☐ OK / ☐ Partiel / ☐ KO | /15 |
| Recherche et filtres | 15% | ☐ OK / ☐ Partiel / ☐ KO | /15 |
| Navigation et ergonomie | 10% | ☐ OK / ☐ Partiel / ☐ KO | /10 |
| Lisibilité des données | 10% | ☐ OK / ☐ Partiel / ☐ KO | /10 |
| **Sous-total fonctionnel** | **100%** | | **/100** |

### 2.3 Performance (20%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Démarrage application | 15% | ☐ < 10s / ☐ 10-20s / ☐ > 20s | /15 |
| Chargement Dashboard | 20% | ☐ < 3s / ☐ 3-5s / ☐ > 5s | /20 |
| Temps réponse CHIFA | 25% | ☐ < 5s / ☐ 5-10s / ☐ > 10s | /25 |
| Stabilité (uptime) | 25% | ☐ > 99% / ☐ 95-99% / ☐ < 95% | /25 |
| Mémoire application | 15% | ☐ < 128MB / ☐ 128-256MB / ☐ > 256MB | /15 |
| **Sous-total performance** | **100%** | | **/100** |

### 2.4 Support (20%)

| Critère | Poids | Résultat | Score |
|---------|-------|----------|-------|
| Temps réponse S1 | 20% | ☐ < 15min / ☐ 15-30min / ☐ > 30min | /20 |
| Temps réponse S2 | 15% | ☐ < 30min / ☐ 30-60min / ☐ > 60min | /15 |
| Satisfaction pharmacien | 30% | ☐ ≥ 4/5 / ☐ 3/5 / ☐ < 3/5 | /30 |
| Nombre d'incidents S1 | 20% | ☐ 0 / ☐ 1 / ☐ > 1 | /20 |
| Résolution incidents | 15% | ☐ Sans impact / ☐ Impact mineur / ☐ Bloquant | /15 |
| **Sous-total support** | **100%** | | **/100** |

---

## 3. Score Final

```
Score Final = (Technique × 30%) + (Fonctionnel × 30%)
            + (Performance × 20%) + (Support × 20%)
```

| Score | Décision |
|-------|----------|
| ≥ 85% | **GO Production** — Tous les critères sont remplis |
| 70–84% | **GO Conditionnel** — Plan d'action requis sur les points < 70% |
| 50–69% | **Prolongation** — 14 jours supplémentaires + correction des écarts |
| < 50% | **NO-GO** — Retour Phase 013/014 pour corrections |

---

## 4. Rapport de Synthèse

### En-tête

```
BM Pharma — Phase 015 — Final Assessment Report
───────────────────────────────────────────────
Pharmacie pilote : [Nom]
Période          : [J-14] → [J+30]
Date rapport     : 2026-07-29
Version testée   : 2.0.0
```

### Résumé Exécutif

```
[Paragraphe de synthèse — à rédiger à J+30]

Exemple :
Le pilote s'est déroulé du [date] au [date] à la pharmacie [nom].
Les critères techniques, fonctionnels, de performance et de support
ont été [validés / partiellement validés / non validés].
[Incidents notables / Aucun incident notable].
Le pharmacien se déclare [satisfait / partiellement satisfait /
insatisfait] de l'application.
```

### Résultats par axe

| Axe | Score | Seuil GO | Statut |
|-----|-------|----------|--------|
| Technique | /100 | ≥ 70 | ☐ Atteint / ☐ Non atteint |
| Fonctionnel | /100 | ≥ 70 | ☐ Atteint / ☐ Non atteint |
| Performance | /100 | ≥ 70 | ☐ Atteint / ☐ Non atteint |
| Support | /100 | ≥ 70 | ☐ Atteint / ☐ Non atteint |
| **Score Final** | **/100** | **≥ 70** | **☐ Atteint / ☐ Non atteint** |

### Incidents

| # | Date | Sévérité | Description | Résolution |
|---|------|----------|-------------|------------|
| 1 | | | | |
| 2 | | | | |

---

## 5. Recommandation Finale

### Décision

```
☐ GO Production
☐ GO Conditionnel
  Conditions : __________________________________
☐ Prolongation Pilote
  Durée : 14 jours
  Motif : ______________________________________
☐ NO-GO
  Motif : ______________________________________
  Actions requises : ____________________________
```

### Plan d'Action (si GO Conditionnel)

| # | Action | Responsable | Échéance |
|---|--------|-------------|----------|
| 1 | | | |
| 2 | | | |

---

## 6. Signatures

### Validation Pharmacie Pilote

```
Nom : ______________________________
Pharmacie : ______________________________
Date : ______________
Signature : ______________________________
```

### Validation Équipe Projet

```
Chef de Projet : ______________________________
Date : ______________
Signature : ______________________________

Directeur Technique : ______________________________
Date : ______________
Signature : ______________________________
```
