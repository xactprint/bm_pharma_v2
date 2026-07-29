# 014-H — Pilot Deployment Checklist

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Objective:** Checklist complète de déploiement d'une pharmacie pilote.

---

## Instructions

- Cocher chaque case après vérification
- Signaler tout écart dans la section "Observations"
- Une case non cochée = un risque pour le pilote
- Ne pas démarrer le pilote tant que toutes les cases ne sont pas vertes

---

## 1. Prérequis Infrastructure

### 1.1 PostgreSQL

- [ ] **PG1** PostgreSQL 9.3.4 (ou ultérieur compatible) est installé
- [ ] **PG2** La base `CHIFA_OFFICINE` existe
- [ ] **PG3** Le schéma `public` contient les tables : `facture`, `detail_fact`, `bordereau`, `medicament`, `parametre`, `signature`
- [ ] **PG4** L'utilisateur `pharm` existe avec droits de connexion
- [ ] **PG5** L'utilisateur `pharm` a les droits SELECT sur toutes les tables CHIFA
- [ ] **PG6** L'utilisateur `pharm` a les droits INSERT/UPDATE sur `facture` et `detail_fact` (si mode Test/Production)
- [ ] **PG7** La connexion réseau au serveur PG est possible (port 5432 ouvert)
- [ ] **PG8** Le service PostgreSQL est en cours d'exécution
- [ ] **PG9** `SHOW server_version;` → version compatible (≥ 9.3)
- [ ] **PG10** La base est accessible depuis le poste client (test avec psql)

**Référence :** `BM-PHASE-004.12-REAL-CONNECTION.md`, `BM-PHASE-004.9-REAL-SCHEMA.md`

### 1.2 CHIFA-OFFICINE

- [ ] **CH1** CHIFA-OFFICINE est installé et fonctionnel
- [ ] **CH2** CHIFA-OFFICINE est en cours d'exécution
- [ ] **CH3** CHIFA-OFFICINE a accès à la même base PostgreSQL
- [ ] **CH4** Les factures écrites par BM Pharma sont visibles dans CHIFA-OFFICINE
- [ ] **CH5** Le chemin d'installation est connu (`ApplicationPath` dans config)

**Référence :** `BM-PHASE-007-G-REAL-WRITE-REPORT.md`

### 1.3 SAM / Token PKCS#11

- [ ] **SM1** Un lecteur de carte PKCS#11 est installé sur le poste
- [ ] **SM2** Le token professionnel (SAM) est présent et fonctionnel
- [ ] **SM3** CHIFA-OFFICINE détecte le token
- [ ] **SM4** La signature est possible depuis CHIFA-OFFICINE

**Référence :** `BM-PHARMA_CHIFA_SECURITY.md`

### 1.4 Certificat

- [ ] **CE1** Le certificat CHIFA est présent et valide
- [ ] **CE2** Le certificat n'est pas expiré
- [ ] **CE3** CHIFA-OFFICINE utilise le certificat correctement

### 1.5 Sauvegarde

- [ ] **SA1** Une sauvegarde complète de `CHIFA_OFFICINE` a été effectuée
- [ ] **SA2** La sauvegarde est stockée dans un emplacement sûr
- [ ] **SA3** La procédure de restauration a été testée
- [ ] **SA4** Une sauvegarde du dossier d'installation BM Pharma a été effectuée

### 1.6 Réseau

- [ ] **RE1** Le poste client a accès au serveur PostgreSQL (ping + port)
- [ ] **RE2** La latence réseau est < 10ms (test : `ping -t <server>`)
- [ ] **RE3** Aucun firewall ne bloque le port 5432
- [ ] **RE4** La connexion est stable (pas de perte de paquets)

### 1.7 Windows

- [ ] **WI1** Windows 10 ou 11 64-bit
- [ ] **WI2** .NET 8 Runtime installé
- [ ] **WI3** Aucun antivirus ne bloque l'application
- [ ] **WI4** L'utilisateur a les droits d'exécution
- [ ] **WI5** Le dossier d'installation est accessible

### 1.8 Permissions

- [ ] **PE1** L'utilisateur Windows a les droits d'écriture dans le dossier de logs
- [ ] **PE2** L'utilisateur Windows a les droits d'écriture dans le dossier SQLite (%APPDATA%)
- [ ] **PE3** Le fichier `appsettings.json` a les permissions restreintes (lecture seule pour les autres utilisateurs)

---

## 2. Application BM Pharma

### 2.1 Installation

- [ ] **BM1** L'application a été installée (MSI ou déploiement manuel)
- [ ] **BM2** Le fichier `appsettings.json` est présent et configuré
- [ ] **BM3** Le mode est positionné sur `ReadOnly` pour la validation initiale
- [ ] **BM4** L'application démarre sans erreur
- [ ] **BM5** Le tableau de bord affiche les statuts corrects

### 2.2 Configuration

- [ ] **CF1** `ConnectionString` pointe vers la bonne base
- [ ] **CF2** `Mode` est correctement positionné
- [ ] **CF3** `ApplicationPath` pointe vers CHIFA-OFFICINE
- [ ] **CF4** Les logs s'écrivent dans le dossier attendu

### 2.3 Validation Fonctionnelle

- [ ] **VF1** Le tableau de bord s'affiche avec des données réelles
- [ ] **VF2** La préparation d'une facture fonctionne (mode ReadOnly)
- [ ] **VF3** La validation d'une facture fonctionne
- [ ] **VF4** L'aperçu avant soumission fonctionne
- [ ] **VF5** La soumission fonctionne (simulée en ReadOnly)
- [ ] **VF6** La liste des bordereaux se charge
- [ ] **VF7** Les actions bordereau sont disponibles
- [ ] **VF8** Le journal d'audit est consultable
- [ ] **VF9** Le rafraîchissement fonctionne (manuel et automatique)

---

## 3. Monitoring

### 3.1 Préparation

- [ ] **MO1** Les logs sont vérifiés (aucune erreur persistante)
- [ ] **MO2** Le Circuit Breaker est en état `Closed`
- [ ] **MO3** Les métriques sont visibles dans le dashboard
- [ ] **MO4** L'ID de corrélation est présent
- [ ] **MO5** La dernière synchronisation est tracée

### 3.2 Alerte

- [ ] **AL1** Les seuils d'alerte sont définis (si applicable)
- [ ] **AL2** Les notifications sont configurées (si applicable)

---

## 4. Tests de Validation

### 4.1 Mode ReadOnly

- [ ] **RO1** L'application démarre en mode ReadOnly
- [ ] **RO2** Le bandeau orange "MODE LECTURE SEULE" est affiché
- [ ] **RO3** Les opérations sont simulées (messages "simulation")
- [ ] **RO4** Aucune écriture PG n'est effectuée (vérifier avec un test)

### 4.2 Mode Production

- [ ] **PR1** Basculer en mode Production (`Mode: "Test"` recommandé d'abord)
- [ ] **PR2** Effectuer une facture test avec vérification dans CHIFA-OFFICINE
- [ ] **PR3** Vérifier que la facture est visible dans CHIFA-OFFICINE
- [ ] **PR4** Vérifier que l'audit log est complet
- [ ] **PR5** Effectuer un rollback si nécessaire (support)

---

## 5. Validation Pilote

### 5.1 J-7 (Pré-validation)

- [ ] **P1** Toutes les cases ci-dessus sont cochées
- [ ] **P2** Un créneau de déploiement est planifié
- [ ] **P3** Le support technique est prévenu
- [ ] **P4** Un canal de communication d'urgence est établi

### 5.2 J-0 (Déploiement)

- [ ] **D1** Backup effectué (PG + config)
- [ ] **D2** Application déployée
- [ ] **D3** Configuration vérifiée
- [ ] **D4** Mode ReadOnly vérifié
- [ ] **D5** Pharmacien formé (cf. Manuel Opérateur)
- [ ] **D6** Test rapide effectué (dashboard + préparation)
- [ ] **D7** Logs vérifiés après 15 minutes d'utilisation
- [ ] **D8** Go du pharmacien obtenu

### 5.3 J+7 (Bilan)

- [ ] **B1** Aucun incident bloquant
- [ ] **B2** Toutes les fonctionnalités utilisées au moins une fois
- [ ] **B3** Feedback pharmacien collecté
- [ ] **B4** Checklist UAT remplie (cf. 014-B)
- [ ] **B5** Décision GO/NOGO confirmée

---

## Résumé

| Section | Cochées | Total | Statut |
|---------|---------|-------|--------|
| 1.1 PostgreSQL | __ | 10 | __% |
| 1.2 CHIFA-OFFICINE | __ | 5 | __% |
| 1.3 SAM/Token | __ | 4 | __% |
| 1.4 Certificat | __ | 3 | __% |
| 1.5 Sauvegarde | __ | 4 | __% |
| 1.6 Réseau | __ | 4 | __% |
| 1.7 Windows | __ | 5 | __% |
| 1.8 Permissions | __ | 3 | __% |
| 2.1 Installation | __ | 5 | __% |
| 2.2 Configuration | __ | 4 | __% |
| 2.3 Validation Fct. | __ | 9 | __% |
| 3. Monitoring | __ | 5 | __% |
| 4. Tests | __ | 9 | __% |
| 5. Validation Pilote | __ | 17 | __% |
| **TOTAL** | **__** | **87** | **__%** |

**Seuil de déploiement :** 100% (87/87) obligatoire.

**Date de déploiement :** _________________

**Responsable déploiement :** _________________

**Pharmacien validateur :** _________________
