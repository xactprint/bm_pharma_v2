# 015-A — Installation Package

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir la stratégie d'installation officielle.

---

## Analysis des Options

| Critère | MSI | MSIX | ClickOnce | Auto-Update |
|---------|-----|------|-----------|-------------|
| Installation silencieuse | ✅ | ✅ | ✅ | ❌ (màj seulement) |
| Désinstallation propre | ✅ | ✅ | ✅ | ✅ |
| GPO/AD déploiement | ✅ | ✅ | ❌ | ❌ |
| Permissions élévées | ✅ (Admin) | ✅ (sandbox) | ❌ (utilisateur) | ✅ (admin) |
| Mise à jour automatique | ❌ (par GPO) | ❌ (Store) | ✅ | ✅ |
| Configuration machine | ✅ (HKLM) | ✅ (sandbox) | ❌ (user) | N/A |
| Rollback version | ✅ (Windows) | ✅ (Windows) | ✅ (manifests) | ✅ |
| Complexité implémentation | Moyenne | Haute | Faible | Faible |
| Utilisateur non-admin | ❌ | ✅ | ✅ | ✅ |
| Contrôle version | ✅ (MSI version) | ✅ (Store) | ✅ (app.config) | ✅ (serveur) |

---

## Décision : MSI + Auto-Update (Hybrid)

### Stratégie officielle

| Composant | Technologie | Justification |
|-----------|------------|---------------|
| **Installation initiale** | **MSI** | Installation silencieuse, GPO, permissions, désinstallation propre |
| **Mises à jour** | **Auto-Update** (custom) | Mises à jour incrémentales sans réinstallation complète |
| **Redistribution** | **ZIP portable** | Backup, déploiement manuel si MSI impossible |

### Pourquoi MSI ?

1. **Installation silencieuse :** `msiexec /i BMPharma.msi /qn` pour déploiement GPO
2. **Désinstallation :** `msiexec /x BMPharma.msi /qn` — complète et propre
3. **Permissions :** Élévation admin garantit que les dossiers `Program Files`, `appsettings.json`, et les logs sont accessibles
4. **Configuration machine :** Registre `HKLM\Software\BMPharma` pour les paramètres globaux
5. **Rollback natif :** Windows Installer restore l'état précédent en cas d'échec

### Pourquoi pas MSIX / ClickOnce ?

- **MSIX :** Overkill pour un déploiement mono-poste en pharmacie. La sandbox complique l'accès aux fichiers locaux (logs, config, SQLite).
- **ClickOnce :** Ne supporte pas l'installation pour "Tous les utilisateurs". La configuration machine n'est pas possible.

---

## Installation Package Specifications

### MSI Package (`BMPharma-Setup-{version}.msi`)

| Property | Value |
|----------|-------|
| **ProductCode** | `{GUID}` (unique per version) |
| **UpgradeCode** | `{FIXED-GUID}` (same for all versions) |
| **Manufacturer** | BM Pharma |
| **ProductName** | BM Pharma v2 |
| **InstallScope** | perMachine |
| **InstallDir** | `ProgramFiles64Folder\BMPharma\` |
| **Target** | `net8.0-windows` |

### Files installed

```
ProgramFiles\BMPharma\
├── BMPharma.UI.exe
├── BMPharma.UI.dll
├── BMPharma.CHIFA.dll
├── BMPharma.Domain.dll
├── BMPharma.Application.dll
├── BMPharma.Infrastructure.dll
├── BMPharma.Shared.dll
├── BMPharma.Persistence.SQLite.dll
├── BMPharma.Persistence.PostgreSQL.dll
├── BMPharma.Notifications.dll
├── BMPharma.CNAS.dll
├── BMPharma.Sync.dll
├── BMPharma.Reporting.dll
├── *.deps.json
├── *.runtimeconfig.json
├── appsettings.json
├── appsettings.Development.json
├── Npgsql.dll
├── Microsoft.*.dll
├── CommunityToolkit.Mvvm.dll
├── Serilog*.dll
├── MaterialDesignThemes*.dll
└── MahApps.Metro*.dll

AppData\BMPharma\
├── bmpharma.db          (SQLite — données locales)
├── logs\                (Serilog)
│   ├── bmpharma-20260729.log
│   └── ...
└── temp\                (fichiers temporaires)

ProgramData\BMPharma\
├── config\              (sur-couche configuration)
│   └── pharmacie.json
└── updates\             (cache mises à jour)
```

### Registry

```
HKLM\Software\BMPharma\
├── InstallPath = "C:\Program Files\BMPharma\"
├── Version = "2.0.0"
└── Mode = "ReadOnly" | "Test" | "Production"
```

---

## Installation Silent

```powershell
# Installation silencieuse (déploiement GPO)
msiexec /i BMPharma-Setup-2.0.0.msi /qn /norestart

# Installation avec paramètres
msiexec /i BMPharma-Setup-2.0.0.msi /qn `
    CHIFA_CONNECTION_STRING="Host=pharma-db;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=***" `
    CHIFA_MODE="ReadOnly"

# Désinstallation silencieuse
msiexec /x {UPGRADE-CODE-GUID} /qn /norestart
```

---

## Auto-Update System

### Version Checking

```
BM Pharma → GET /update/check?version=2.0.0
Serveur   → 200 OK { latest: "2.0.1", url: "...", hash: "...", mandatory: false }
```

### Update Files

```
BMPharma-Update-{version}.zip
├── BMPharma.UI.exe
├── BMPharma.CHIFA.dll
├── ... (only changed files)
└── update-manifest.json
```

### Update Process

1. Check version on server (configurable URL)
2. Download update ZIP to `ProgramData\BMPharma\updates\`
3. Verify hash
4. Backup current installation to `ProgramData\BMPharma\updates\backup\`
5. Apply update (replace files)
6. Restart application
7. On failure: restore from backup (automatic rollback)

### update-manifest.json

```json
{
  "version": "2.0.1",
  "minVersion": "2.0.0",
  "mandatory": false,
  "allowDowngrade": false,
  "requiresRestart": true,
  "description": "Correction mineure",
  "files": ["BMPharma.UI.exe", "BMPharma.CHIFA.dll"]
}
```

---

## ZIP Portable Distribution

Créer un package de secours pour déploiement manuel :

```powershell
Compress-Archive -Path "C:\Program Files\BMPharma\*" -DestinationPath "BMPharma-Portable-2.0.0.zip"
```

Usage : dézipper, exécuter `BMPharma.UI.exe`. Fichier `appsettings.json` inclus.

---

## Désinstallation

| Méthode | Commande |
|---------|----------|
| Programs & Features | GUI Windows |
| MSI silent | `msiexec /x {UPGRADE-CODE-GUID} /qn` |
| Manual (portable) | Delete folder + `%APPDATA%\BMPharma` + `ProgramData\BMPharma` |

### Cleanup complet

```powershell
# Supprimer toutes les données
Remove-Item -Path "$env:ProgramFiles\BMPharma" -Recurse -Force
Remove-Item -Path "$env:APPDATA\BMPharma" -Recurse -Force
Remove-Item -Path "$env:ProgramData\BMPharma" -Recurse -Force
Remove-Item -Path "HKLM:\Software\BMPharma" -Recurse -Force
```
