# 015-F — Support Bundle

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir le package de diagnostic générable automatiquement pour le support technique.

---

## Overview

Le Support Bundle est un fichier ZIP généré par l'application (ou par un script CLI) contenant toutes les informations nécessaires au diagnostic d'un problème. Il ne contient **aucune donnée patient**.

### Generation

```powershell
# CLI
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --support-bundle

# UI
Bouton "Générer le bundle de support" dans le Diagnostic Center
```

### Output

```
BMPharma-Support-Bundle-{timestamp}-{correlationId}.zip
```

---

## Bundle Contents

```
BMPharma-Support-Bundle-20260729-183000-a1b2c3d4.zip
├── 00-manifest.json                          # Bundle metadata
├── 01-configuration/
│   ├── appsettings.json                      # Copie de la config
│   ├── pharmacie.json                        # Sur-couche machine (si existe)
│   └── configuration-summary.txt             # Résumé lisible
├── 02-versions/
│   ├── bmpharma-version.txt                  # Version + build
│   ├── dotnet-runtime.txt                    # dotnet --list-runtimes
│   └── pg-version.txt                        # SELECT version()
├── 03-logs/
│   ├── bmpharma-current.log                  # Log du jour
│   └── bmpharma-previous.log                 # Log de la veille
├── 04-postgresql/
│   ├── pg-health.txt                         # Health check result
│   ├── pg-tables.txt                         # Liste des tables + row counts
│   ├── pg-connections.txt                    # Connexions actives
│   └── pg-locks.txt                          # Verrous actifs
├── 05-chifa/
│   ├── chifa-status.txt                      # CHIFA available? running?
│   ├── chifa-mode.txt                        # ReadOnly/Test/Production
│   └── chifa-token.txt                       # Token status
├── 06-monitoring/
│   ├── circuit-breaker.txt                   # État CB par clé
│   ├── metrics.txt                           # Métriques agrégées
│   ├── health-check.txt                      # Derniers health checks
│   └── operations-summary.txt                # Résumé opérations
├── 07-diagnostics/
│   ├── diagnostic-quick.json                 # Résultat diagnostic (JSON)
│   ├── diagnostic-full.json                  # Résultat diagnostic complet
│   └── errors-summary.txt                    # Dernières erreurs
└── 08-system/
    ├── os-info.txt                           # Windows version
    ├── processes.txt                         # Processus en cours
    ├── disk-space.txt                        # Espace disque
    └── network.txt                           # Connexions réseau
```

---

## File Specifications

### 00-manifest.json

```json
{
  "bundleVersion": "1.0",
  "generatedAt": "2026-07-29T18:30:00Z",
  "applicationVersion": "2.0.0",
  "correlationId": "a1b2c3d4e5f6",
  "mode": "ReadOnly",
  "generatedBy": "pharmacist@pharma-pc",
  "machineName": "PHARMA-PC-01",
  "files": [
    "01-configuration/appsettings.json",
    "02-versions/bmpharma-version.txt",
    "03-logs/bmpharma-current.log"
  ],
  "containsPatientData": false
}
```

### 01-configuration/configuration-summary.txt

```
=== Configuration Summary ===
Mode: ReadOnly
PostgreSQL Host: localhost:5432
PostgreSQL Database: CHIFA_OFFICINE
PostgreSQL User: pharm
CHIFA Path: C:\Program Files\CHIFA-OFFICINE
Auto Refresh: 30s
Health Check: 60s
```

### 02-versions/bmpharma-version.txt

```
BM Pharma Version: 2.0.0
Build: 20260729.1
Commit: abc123def456
Framework: .NET 8.0.6
OS: Microsoft Windows 10 64-bit
```

### 04-postgresql/pg-health.txt

```
=== PostgreSQL Health ===
Host: localhost:5432
Database: CHIFA_OFFICINE
Status: Connected
Response Time: 12ms
Server Version: PostgreSQL 9.3.4
Tables: 12 (6 CHIFA, 6 public)
Active Connections: 3
Locks: 0
```

### 06-monitoring/circuit-breaker.txt

```
=== Circuit Breaker Status ===
Key: ChifaIntegration
State: Closed
Failure Count: 0
Last Failure: N/A
Open Timeout: 30000ms
Success Threshold: 2
Failure Threshold: 3
```

### 06-monitoring/metrics.txt

```
=== Metrics Summary ===
Total Operations: 42
Successful: 40
Failed: 2
Average Duration: 45ms
Last Operation: GetDashboardOverview
Last Sync: 2026-07-29T18:29:30Z
Last Sync Result: 0 fact., 0 bord.
```

### 07-diagnostics/errors-summary.txt

```
=== Last 10 Errors ===
1. [2026-07-29 17:00:12] ERR: Invoice preparation failed - Timeout expired (CID: f6e5d4c3b2a1)
2. [2026-07-29 16:45:33] WRN: PostgreSQL connection attempt timed out (CID: a1b2c3d4e5f6)
...
```

---

## Generation Script (PowerShell)

```powershell
# generate-support-bundle.ps1
param(
    [string]$OutputDir = "$env:TEMP\BMPharma-Bundles",
    [string]$AppDir = "$env:ProgramFiles\BMPharma",
    [string]$AppDataDir = "$env:APPDATA\BMPharma",
    [string]$ProgramDataDir = "$env:ProgramData\BMPharma"
)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$bundleName = "BMPharma-Support-Bundle-$timestamp"
$bundleDir = Join-Path $OutputDir $bundleName

# Create directory structure
$dirs = @(
    "01-configuration", "02-versions", "03-logs",
    "04-postgresql", "05-chifa", "06-monitoring",
    "07-diagnostics", "08-system"
)
foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Path (Join-Path $bundleDir $dir) -Force | Out-Null
}

# 01 — Configuration
Copy-Item "$AppDir\appsettings.json" "$bundleDir\01-configuration\" -ErrorAction SilentlyContinue
if (Test-Path "$ProgramDataDir\config\pharmacie.json") {
    Copy-Item "$ProgramDataDir\config\pharmacie.json" "$bundleDir\01-configuration\"
}

# 02 — Versions
"BM Pharma Version: 2.0.0" | Out-File "$bundleDir\02-versions\bmpharma-version.txt"
dotnet --list-runtimes 2>$null | Out-File "$bundleDir\02-versions\dotnet-runtime.txt"

# 03 — Logs
Get-ChildItem "$AppDataDir\logs\*.log" | Sort-Object LastWriteTime -Descending | 
    Select-Object -First 2 | ForEach-Object {
        Copy-Item $_.FullName "$bundleDir\03-logs\"
    }

# 04 — PostgreSQL
# (nécessite psql accessible ou les données du health check)

# 08 — System
"OS: $((Get-CimInstance Win32_OperatingSystem).Caption)" | Out-File "$bundleDir\08-system\os-info.txt"
Get-Process | Out-File "$bundleDir\08-system\processes.txt"
Get-PSDrive -PSProvider FileSystem | Out-File "$bundleDir\08-system\disk-space.txt"

# Generate manifest
$manifest = @{
    bundleVersion = "1.0"
    generatedAt = (Get-Date -Format "o")
    applicationVersion = "2.0.0"
    correlationId = [guid]::NewGuid().ToString("N").Substring(0, 12)
    machineName = $env:COMPUTERNAME
}
$manifest | ConvertTo-Json | Out-File "$bundleDir\00-manifest.json"

# Create ZIP
Compress-Archive -Path "$bundleDir\*" -DestinationPath "$bundleDir.zip"
Remove-Item $bundleDir -Recurse -Force

Write-Output "Support bundle generated: $bundleDir.zip"
```

---

## Data Privacy

### Included
- ✅ Versions (application, .NET, OS)
- ✅ Configuration (sans mots de passe)
- ✅ Logs (sans données patient)
- ✅ Métriques agrégées
- ✅ Statistiques (counts, durées)
- ✅ N° de version, build

### Excluded
- ❌ Mots de passe (ConnectionString filtré)
- ❌ Token PKCS#11
- ❌ Noms de patients
- ❌ Adresses
- ❌ N° de sécurité sociale
- ❌ Contenu des factures
- ❌ Données de vente

### Filtrage automatique

```powershell
# Filtrer les mots de passe dans la configuration
$config = Get-Content "$bundleDir\01-configuration\appsettings.json" -Raw
$config = $config -replace '"Password":\s*"[^"]*"', '"Password": "***"'
$config | Set-Content "$bundleDir\01-configuration\appsettings.json"
```
