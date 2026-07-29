# 015-H — Pilot Deployment Procedure

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Procédure officielle de déploiement pilote.

---

## Overview

```
J-14 ───── J-7 ───── J-3 ───── J-0 ───── J+7 ───── J+14 ───── J+30
  │         │         │         │         │         │          │
  ▼         ▼         ▼         ▼         ▼         ▼          ▼
Prépa    Pré-     Pré-     Déploie-  ReadOnly  Test     Bilan
         validation        ment      (J0-J7)   (J7-J14) (J30)
                   Tests    J-0
```

---

## Phase 1 — Préparation (J-14 à J-7)

### Administration

- [ ] **P1.1** Contacter la pharmacie pilote
- [ ] **P1.2** Planifier le déploiement
- [ ] **P1.3** Vérifier que le poste répond aux prérequis (015-H checklist)
- [ ] **P1.4** Installer PostgreSQL 9.3.4 si nécessaire
- [ ] **P1.5** Installer CHIFA-OFFICINE si nécessaire
- [ ] **P1.6** Vérifier que la base CHIFA_OFFICINE est accessible
- [ ] **P1.7** Créer l'utilisateur PostgreSQL `pharm` avec les droits appropriés
- [ ] **P1.8** Effectuer une sauvegarde complète de CHIFA_OFFICINE
- [ ] **P1.9** Préparer le fichier `appsettings.json` pour la pharmacie

### Support

- [ ] **P1.10** Créer le compte support de la pharmacie pilote
- [ ] **P1.11** Préparer le canal de support (téléphone, email)
- [ ] **P1.12** Planifier la formation pharmacien (2h)

---

## Phase 2 — Pré-validation Environnement (J-7 à J-3)

### Tests d'environnement

- [ ] **P2.1** Exécuter le Diagnostic Center (`--full`)
- [ ] **P2.2** Vérifier les 18 tests du Diagnostic Center
- [ ] **P2.3** Tester la connexion PostgreSQL
- [ ] **P2.4** Tester la connexion CHIFA
- [ ] **P2.5** Vérifier les permissions Windows
- [ ] **P2.6** Vérifier l'accès aux logs
- [ ] **P2.7** Tester le mode ReadOnly

### Correction

- [ ] **P2.8** Corriger tout échec du Diagnostic Center
- [ ] **P2.9** Re-exécuter le Diagnostic Center après correction
- [ ] **P2.10** Documenter les problèmes rencontrés

---

## Phase 3 — Installation BM Pharma (J-3 à J-0)

### Installation

- [ ] **P3.1** Copier `BMPharma-Setup-{version}.msi` sur le poste
- [ ] **P3.2** Lancer l'installation
- [ ] **P3.3** Vérifier que l'installation s'est terminée sans erreur
- [ ] **P3.4** Vérifier que le dossier `Program Files\BMPharma\` contient tous les fichiers

### Configuration

- [ ] **P3.5** Copier `appsettings.json` avec les paramètres de la pharmacie
- [ ] **P3.6** Vérifier que `Mode` = `ReadOnly`
- [ ] **P3.7** Vérifier `ConnectionString` (hôte, port, base, utilisateur)
- [ ] **P3.8** Vérifier `ApplicationPath` (chemin CHIFA-OFFICINE)

### Premier Lancement

- [ ] **P3.9** Lancer BM Pharma
- [ ] **P3.10** Vérifier que l'application démarre sans erreur
- [ ] **P3.11** Vérifier la status bar : "Prêt" + "CHIFA: Connecté" (ou "Déconnecté" si CHIFA arrêté)
- [ ] **P3.12** Vérifier le Diagnostic Center automatique

### Validation Fonctionnelle

- [ ] **P3.13** Dashboard : vérifier les 6 cartes de statut
- [ ] **P3.14** Dashboard : vérifier les 8 étapes du workflow
- [ ] **P3.15** Dashboard : vérifier les métriques de monitoring
- [ ] **P3.16** Dashboard : vérifier l'état du Circuit Breaker
- [ ] **P3.17** Préparation facture : ajouter des lignes, vérifier les montants
- [ ] **P3.18** Préparation facture : valider (simulé en ReadOnly)
- [ ] **P3.19** Préparation facture : aperçu avant soumission
- [ ] **P3.20** Préparation facture : soumettre (simulé en ReadOnly)
- [ ] **P3.21** Bordereau : vérifier la liste des bordereaux
- [ ] **P3.22** Bordereau : vérifier les actions disponibles
- [ ] **P3.23** Audit : vérifier le journal d'audit après les opérations

---

## Phase 4 — Formation Pharmacien (J-0)

### Sessions

- [ ] **P4.1** Présenter l'architecture globale (15 min)
- [ ] **P4.2** Parcourir le manuel opérateur (014-F) (30 min)
- [ ] **P4.3** Démonstration du workflow complet (30 min)
- [ ] **P4.4** Parcourir la checklist UAT (014-B) (30 min)
- [ ] **P4.5** Répondre aux questions (15 min)

### Validation Pharmacien

- [ ] **P4.6** Le pharmacien sait ouvrir le dashboard
- [ ] **P4.7** Le pharmacien sait préparer une facture
- [ ] **P4.8** Le pharmacien sait consulter les bordereaux
- [ ] **P4.9** Le pharmacien sait lire les métriques de monitoring
- [ ] **P4.10** Le pharmacien sait interpréter le Circuit Breaker
- [ ] **P4.11** Le pharmacien sait générer un support bundle
- [ ] **P4.12** Le pharmacien a signé la validation de formation

---

## Phase 5 — Pilote ReadOnly (J+0 à J+7)

### Démarrage

- [ ] **P5.1** Le pharmacien commence à utiliser BM Pharma en ReadOnly
- [ ] **P5.2** L'administrateur surveille les logs quotidiennement
- [ ] **P5.3** Aucune écriture dans CHIFA_OFFICINE (vérifié)

### Suivi Quotidien

- [ ] **P5.4** Logs vérifiés (aucune erreur persistante)
- [ ] **P5.5** Dashboard métriques vérifiées
- [ ] **P5.6** Circuit Breaker toujours fermé
- [ ] **P5.7** Backup automatique vérifié

### Problèmes

- [ ] **P5.8** Tout problème est documenté
- [ ] **P5.9** Les problèmes bloquants sont corrigés immédiatement
- [ ] **P5.10** Les problèmes non-bloquants sont documentés pour la prochaine version

---

## Phase 6 — Validation (J+7)

### Réunion de validation

- [ ] **P6.1** Pharmacien : retour sur l'utilisation
- [ ] **P6.2** Checklist UAT (014-B) remplie
- [ ] **P6.3** Score UAT ≥ 90%
- [ ] **P6.4** Aucun blocant non résolu
- [ ] **P6.5** Tous les problèmes documentés

### Décision

- [ ] **P6.6** ❌ Retour ReadOnly (prolongation J+7)
- [ ] **P6.7** ✅ Passer en mode Test (écritures réelles)
- [ ] **P6.8** ⏸ Pause pilote (retour à la phase précédente)

---

## Phase 7 — Pilote Test (J+7 à J+14)

### Activation

- [ ] **P7.1** Modifier `appsettings.json` : `"Mode": "Test"`
- [ ] **P7.2** Redémarrer BM Pharma
- [ ] **P7.3** Vérifier que le bandeau ReadOnly a disparu
- [ ] **P7.4** Vérifier que les écritures sont possibles

### Tests d'écriture

- [ ] **P7.5** Créer une facture test
- [ ] **P7.6** Vérifier la facture dans CHIFA-OFFICINE
- [ ] **P7.7** Vérifier l'audit log
- [ ] **P7.8** Vérifier les métriques
- [ ] **P7.9** Vérifier que le Circuit Breaker reste fermé
- [ ] **P7.10** Supprimer la facture test de CHIFA_OFFICINE (manuellement)

### Suivi

- [ ] **P7.11** Surveillance des logs
- [ ] **P7.12** Vérification des sauvegardes
- [ ] **P7.13** Aucune anomalie

---

## Phase 8 — Bilan Pilote (J+30)

### Critères d'acceptation (cf. 015-K)

- [ ] **P8.1** 30 jours d'utilisation
- [ ] **P8.2** 0 corruption de données
- [ ] **P8.3** 0 perte de données
- [ ] **P8.4** 100% des rollbacks réussis
- [ ] **P8.5** 100% des sauvegardes effectuées
- [ ] **P8.6** Disponibilité > 99%
- [ ] **P8.7** Temps réponse < 2 secondes
- [ ] **P8.8** Aucune anomalie critique

### Décision finale

- [ ] **P8.9** ✅ GO — Déploiement généralisé
- [ ] **P8.10** ⏸ Retour développement (prolongation pilote)
- [ ] **P8.11** ❌ NO GO — Arrêt du projet

---

## Quick Reference — Day 1 Checklist

```markdown
## J-0 Checklist

### Before pharmacist arrives
□ MSI installed
□ Configuration set (ReadOnly)
□ Diagnostic Center: ALL GREEN
□ Pharmacie.json deployed (if applicable)
□ Backup performed

### During training (2 hours)
□ Architecture overview
□ Manual walkthrough
□ Dashboard demo
□ Invoice preparation demo
□ Bordereau status demo
□ Q&A

### After training
□ Pharmacist completed UAT checklist
□ Training validated (signed)
□ Support contact shared
□ Next check-in scheduled (J+2)
```

---

## Contacts d'Urgence

| Rôle | Contact | Disponibilité |
|------|---------|---------------|
| Support technique | support@bmpharma.com | J-14 à J+30 |
| Administrateur système | admin@pharmacie.com | Lundi-Vendredi 8h-18h |
| Escalade technique | +213 XXX XX XX XX | Urgence uniquement |
