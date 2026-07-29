# 016-A — Clean Environment Deployment

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Installer BM Pharma sur machine vierge et vérifier installation, dépendances, configuration, lancement, désinstallation.

---

## Environnement

| Champ | Valeur |
|-------|--------|
| **Machine** | |
| **OS** | Windows 10/11 64-bit |
| **RAM** | ≥ 4 GB |
| **Disque** | ≥ 500 MB libres |
| **PostgreSQL** | 9.3.4 |
| **CHIFA** | OFFICINE installé et configuré |
| **.NET Runtime** | 8.0 (x64) |
| **Auteur test** | |
| **Date** | |

---

## 1. Installation

### 1.1 MSI Installation Silencieuse

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 1.1.1 | Lancer `msiexec /i BMPharma.msi /qn` | Installation silencieuse sans erreur | ☐ Succès / ☐ Échec |
| 1.1.2 | Vérifier présence dans `Program Files\BMPharma\` | Dossier créé avec tous les fichiers | ☐ OK / ☐ KO |
| 1.1.3 | Vérifier présence dans Menu Démarrer | Raccourci BM Pharma créé | ☐ OK / ☐ KO |
| 1.1.4 | Vérifier présence dans Programmes & Fonctionnalités | BM Pharma listé | ☐ OK / ☐ KO |

### 1.2 Installation Interactive

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 1.2.1 | Lancer `msiexec /i BMPharma.msi` | Assistant d'installation s'affiche | ☐ Succès / ☐ Échec |
| 1.2.2 | Suivre les étapes jusqu'à complétion | Installation réussie | ☐ Succès / ☐ Échec |

---

## 2. Vérification Dépendances

| # | Dépendance | Vérification | Résultat |
|---|-----------|-------------|----------|
| 2.1 | .NET Runtime 8.0 x64 | `dotnet --list-runtimes` | ☐ Présent / ☐ Absent |
| 2.2 | PostgreSQL 9.3.4 | `pg_isready -h localhost -p 5432` | ☐ OK / ☐ KO |
| 2.3 | CHIFA-OFFICINE | Process `CHIFA.exe` en cours | ☐ OK / ☐ KO |
| 2.4 | Windows 10+ 64-bit | `[Environment]::OSVersion.Version` ≥ 10.0 | ☐ OK / ☐ KO |
| 2.5 | Droits utilisateur | Utilisateur a les droits Lecture/Écriture dans `%ProgramData%\BMPharma` | ☐ OK / ☐ KO |

---

## 3. Configuration

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 3.1 | Copier `appsettings.json` dans `%ProgramFiles%\BMPharma\` | Fichier présent | ☐ OK / ☐ KO |
| 3.2 | Vérifier chaîne connexion PostgreSQL | `Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm` | �at OK / ☐ KO |
| 3.3 | Vérifier chaîne connexion CHIFA | `Provider=MSDAORA;Data Source=CHIFA_SRV` | ☐ OK / ☐ KO |
| 3.4 | Vérifier `Monitoring:RefreshIntervalSeconds` | 60 | ☐ OK / ☐ KO |

---

## 4. Lancement

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 4.1 | Lancer BM Pharma depuis le Menu Démarrer | Application démarre sans erreur | ☐ Succès / ☐ Échec |
| 4.2 | Logo BM Pharma affiché | Splash screen visible | ☐ OK / ☐ KO |
| 4.3 | Connexion PostgreSQL établie | Dashboard affiche "Connecté" | ☐ OK / ☐ KO |
| 4.4 | Connexion CHIFA établie | Dashboard affiche "Connecté" | ☐ OK / ☐ KO |
| 4.5 | Données affichées dans le Dashboard | Stocks, factures, clients visibles | ☐ OK / ☐ KO |
| 4.6 | Vérifier `%ProgramData%\BMPharma\logs\` | Fichier de log créé | ☐ OK / ☐ KO |

---

## 5. Désinstallation

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 5.1 | `msiexec /x BMPharma.msi /qn` | Désinstallation silencieuse | ☐ Succès / ☐ Échec |
| 5.2 | Vérifier `Program Files\BMPharma\` supprimé | Dossier supprimé | ☐ OK / ☐ KO |
| 5.3 | Vérifier Menu Démarrer | Raccourci supprimé | ☐ OK / ☐ KO |
| 5.4 | Vérifier Programmes & Fonctionnalités | BM Pharma plus listé | ☐ OK / ☐ KO |
| 5.5 | Vérifier `%ProgramData%\BMPharma\` conservé (données utilisateur) | Dossier intact | ☐ OK / ☐ KO |

---

## 6. Réinstallation

| # | Action | Résultat Attendu | Résultat |
|---|--------|------------------|----------|
| 6.1 | Réinstaller avec `msiexec /i BMPharma.msi /qn` | Installation réussie | ☐ Succès / ☐ Échec |
| 6.2 | Lancer BM Pharma | Application fonctionne | ☐ OK / ☐ KO |
| 6.3 | Vérifier que les données utilisateur sont préservées | `%ProgramData%\BMPharma` intact | ☐ OK / ☐ KO |

---

## Anomalies

| # | Description | Impact | Criticité |
|---|-------------|--------|-----------|
| | | | |
| | | | |

---

## Conclusion

| Critère | Résultat |
|---------|----------|
| Installation MSI | ☐ OK / ☐ KO |
| Dépendances | ☐ OK / ☐ KO |
| Configuration | ☐ OK / ☐ KO |
| Lancement | ☐ OK / ☐ KO |
| Désinstallation | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
