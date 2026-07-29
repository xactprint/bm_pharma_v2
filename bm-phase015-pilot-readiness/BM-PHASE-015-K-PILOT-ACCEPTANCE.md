# 015-K — Pilot Acceptance

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir les critères d'acceptation et le processus de validation du pilote par la pharmacie.

---

## 1. Overview

Le pilote est validé en deux phases :
- **J+7** — Fin de la période ReadOnly + supervision renforcée
- **J+14** — Bilan intermédiaire et poursuite
- **J+30** — Acceptation finale et décision GO / NO-GO production

---

## 2. Conditions Préalables (Issues de Phase 014)

Ces conditions doivent être remplies avant le début du pilote :

| # | Condition | Vérifié par | Statut |
|---|-----------|-------------|--------|
| C1 | ReadOnly pendant 7 jours (consultation uniquement) | Config app | ☐ |
| C2 | Supervision quotidienne par l'équipe support | Support | ☐ |
| C3 | PostgreSQL 9.3.4 installé et configuré | Diagnostic Center | ☐ |
| C4 | Formation pharmacien réalisée (2h) | Support | ☐ |
| C5 | Backup automatisé configuré et testé | Diagnostic Center | ☐ |
| C6 | Production après J+7 uniquement si validation | Décision | ☐ |

---

## 3. Critères d'Acceptation

### C1 — Installation et Configuration

| # | Critère | Méthode | Attendue |
|---|---------|---------|----------|
| C1.1 | Installation MSI réussie | Pharmacien | Succès |
| C1.2 | Configuration PostgreSQL OK | Diagnostic Center T03 | Connecté |
| C1.3 | Configuration CHIFA OK | Diagnostic Center T05 | Connecté |
| C1.4 | Synchronisation OK | Dashboard | Statut vert |
| C1.5 | Backup automatique configuré | Vérification planificateur | OK |

### C2 — Fonctionnalités Métier

| # | Critère | Méthode | Attendue |
|---|---------|---------|----------|
| C2.1 | Dashboard affiche les données CHIFA | Ouverture app | Données visibles |
| C2.2 | Consultation des stocks | Navigation | Lecture seule |
| C2.3 | Consultation des factures | Navigation | Lecture seule |
| C2.4 | Consultation des clients | Navigation | Lecture seule |
| C2.5 | Recherche fonctionnelle | Test | Résultats corrects |
| C2.6 | Filtres et tris fonctionnent | Test | Résultats corrects |

### C3 — ReadOnly (J+0 à J+7)

| # | Critère | Méthode | Attendue |
|---|---------|---------|----------|
| C3.1 | Aucune écriture en base CHIFA | Vérification logs | 0 écriture |
| C3.2 | Message ReadOnly visible | UI | Badge/bandeau |
| C3.3 | Boutons d'écriture désactivés | UI | Grisés |
| C3.4 | Confirmation pharmacien (J+7) | Entretien | Pharmacien OK |

### C4 — Performance

| # | Critère | Seuil | Méthode |
|---|---------|-------|---------|
| C4.1 | Temps démarrage application | < 10s | Chronomètre |
| C4.2 | Temps chargement Dashboard | < 3s | Dashboard |
| C4.3 | Temps réponse CHIFA | < 5s | Avis pharmacien |
| C4.4 | Temps réponse recherche | < 2s | Avis pharmacien |
| C4.5 | Mémoire application | < 256 MB | Diagnostic Center |

### C5 — Stabilité

| # | Critère | Seuil | Méthode |
|---|---------|-------|---------|
| C5.1 | Disponibilité application | > 99% | Logs |
| C5.2 | Disponibilité CHIFA | > 99% | Diagnostic Center |
| C5.3 | Disponibilité PostgreSQL | > 99% | Diagnostic Center |
| C5.4 | Erreurs non-bloquantes | < 5/semaine | Logs |
| C5.5 | Blocage métier | 0 | Pharmacien |

### C6 — Support

| # | Critère | Méthode | Attendue |
|---|---------|---------|----------|
| C6.1 | Temps réponse S1 | < 15 min | Logs support |
| C6.2 | Temps réponse S2 | < 30 min | Logs support |
| C6.3 | Temps réponse S3 | < 2h | Logs support |
| C6.4 | Pharmacien satisfait du support | Questionnaire | ≥ 4/5 |

### C7 — Sauvegarde et Restauration

| # | Critère | Méthode | Attendue |
|---|---------|---------|----------|
| C7.1 | Backup PostgreSQL OK | Vérification fichier | Quotidien |
| C7.2 | Backup SQLite OK | Vérification fichier | Quotidien |
| C7.3 | Test restauration (J+7) | Restaure et vérifie | Succès |

---

## 4. Validation Process

### Phase 1 — J+0 Déploiement

```
☐ Installation MSI réussie
☐ Configuration PostgreSQL vérifiée
☐ Configuration CHIFA vérifiée
☐ Dashboard opérationnel
☐ Mode ReadOnly confirmé
☐ Backup initial effectué
☐ Pharmacien formé
```

### Phase 2 — J+7 Fin ReadOnly

```
☐ Aucune écriture détectée en ReadOnly
☐ Test restauration effectué
☐ Pharmacien confirme : consultation OK
☐ Aucun incident S1/S2 en cours
☐ Décision de passage en production
```

### Phase 3 — J+14 Bilan intermédiaire

```
☐ Application fonctionnelle en production (J+7 à J+14)
☐ Taux d'erreurs < seuil
☐ Performance satisfaisante
☐ Support réactif
☐ Pharmacien satisfait
```

### Phase 4 — J+30 Acceptation finale

```
☐ Tous critères C1-C7 validés
☐ 30 jours sans incident S1
☐ Pharmacien signe l'acceptation
☐ Rapport final produit
```

---

## 5. Questionnaire Pharmacien (J+14)

| Question | Note (1-5) | Commentaire |
|----------|------------|-------------|
| L'application répond-elle rapidement ? | ☐☐☐☐☐ | |
| Les données CHIFA sont-elles correctes ? | ☐☐☐☐☐ | |
| La navigation est-elle intuitive ? | ☐☐☐☐☐ | |
| La recherche est-elle efficace ? | ☐☐☐☐☐ | |
| Le support est-il réactif ? | ☐☐☐☐☐ | |
| Recommanderiez-vous l'application ? | ☐☐☐☐☐ | |

**Note minimale pour acceptation :** ≥ 4/5 sur chaque critère.

---

## 6. Sign-Off

### Acceptation Pharmacie Pilote

```
Pharmacie : ______________________________
Pharmacien : ______________________________
Date : ______________

☐ J'accepte l'application BM Pharma pour une utilisation en production
☐ Je confirme que les critères d'acceptation sont remplis
☐ Je confirme avoir reçu la formation nécessaire

Signature : ______________________________
```

### Validation Équipe Projet

```
Responsable Projet : ______________________________
Date : ______________

☐ Je confirme que le pilote est concluant
☐ Je recommande le déploiement en production

Signature : ______________________________
```

---

## 7. Décisions Possibles

| Décision | Condition | Action |
|----------|-----------|--------|
| **GO Production** | Tous critères validés | Déploiement production immédiat |
| **GO Conditionnel** | Critères OK sauf points mineurs | Plan d'action + suivi |
| **Prolongation** | Critères partiellement OK | 14 jours supplémentaires |
| **NO-GO** | Critères non atteints ou incident S1 | Retour Phase 013/014 |
