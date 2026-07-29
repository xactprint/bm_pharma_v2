# 015-D — Backup Strategy

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir la stratégie complète de sauvegarde et restauration.

---

## Backup Scope

| Composant | Méthode | Fréquence | Rétention | Priorité |
|-----------|---------|-----------|-----------|----------|
| PostgreSQL CHIFA_OFFICINE | `pg_dump` | Quotidienne | 7 jours | CRITICAL |
| SQLite BM Pharma | Copie fichier | Quotidienne | 7 jours | HIGH |
| Configuration | Copie fichiers | Hebdomadaire | 30 jours | MEDIUM |
| Logs | Archive ZIP | Hebdomadaire | 30 jours | LOW |
| Installation MSI | Archive | À chaque version | Permanente | MEDIUM |

---

## 1. PostgreSQL Backup

### Automatic Backup (`pg_dump`)

```powershell
# backup-pg.ps1 — Planifié via Windows Task Scheduler (quotidien 02:00)
param(
    [string]$Host = "localhost",
    [int]$Port = 5432,
    [string]$Db = "CHIFA_OFFICINE",
    [string]$User = "pharm",
    [string]$Password,
    [string]$TargetDir = "$env:ProgramData\BMPharma\backups\pg"
)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$filename = "$Db-$timestamp.backup"
$filepath = Join-Path $TargetDir $filename

# Ensure target directory
New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null

# pg_dump with compression
$env:PGPASSWORD = $Password
& "pg_dump" -h $Host -p $Port -U $User -F c -b -v -f $filepath $Db 2>&1 | Out-File "$filepath.log"

# Verify backup
$file = Get-Item $filepath
if ($file.Length -gt 1KB) {
    Write-Output "Backup successful: $filepath ($($file.Length) bytes)"
} else {
    Write-Error "Backup file too small or empty: $filepath"
}
```

### Compression

- Format `pg_dump -F c` (custom format, compressé par défaut)
- Ratio typique : 5:1 à 10:1
- Taille estimée pour 1 Go de données : ~100-200 Mo

### Rotation

```powershell
# Nettoyage des backups de plus de 7 jours
$retentionDays = 7
Get-ChildItem -Path $TargetDir -Filter "*.backup" | 
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-$retentionDays) } |
    Remove-Item -Force
```

---

## 2. SQLite Backup

### Automatic Backup

```powershell
# backup-sqlite.ps1
$source = "$env:APPDATA\BMPharma\bmpharma.db"
$targetDir = "$env:ProgramData\BMPharma\backups\sqlite"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

New-Item -ItemType Directory -Path $targetDir -Force | Out-File

# Vérifier que l'application est fermée (SQLite ne supporte pas le backup à chaud)
if (Get-Process -Name "BMPharma.UI" -ErrorAction SilentlyContinue) {
    Write-Warning "BM Pharma is running. Backup may be inconsistent."
}

Copy-Item -Path $source -Destination "$targetDir\bmpharma-$timestamp.db" -Force
Compress-Archive -Path "$targetDir\bmpharma-$timestamp.db" -DestinationPath "$targetDir\bmpharma-$timestamp.zip"
Remove-Item "$targetDir\bmpharma-$timestamp.db" -Force
```

### SQLite Online Backup (future)

```csharp
// Backup à chaud via SQLite backup API
using var source = new SqliteConnection("Data Source=bmpharma.db");
using var dest = new SqliteConnection("Data Source=bmpharma-backup.db");
source.Open();
dest.Open();
source.BackupDatabase(dest);
```

---

## 3. Configuration Backup

```powershell
# backup-config.ps1 — Hebdomadaire
$configDir = "$env:ProgramFiles\BMPharma"
$targetDir = "$env:ProgramData\BMPharma\backups\config"
$timestamp = Get-Date -Format "yyyyMMdd"

New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

$files = @("appsettings.json", "appsettings.Development.json")
foreach ($file in $files) {
    $path = Join-Path $configDir $file
    if (Test-Path $path) {
        Copy-Item -Path $path -Destination "$targetDir\$file.$timestamp" -Force
    }
}
```

---

## 4. Log Archive

```powershell
# archive-logs.ps1 — Hebdomadaire
$logDir = "$env:APPDATA\BMPharma\logs"
$targetDir = "$env:ProgramData\BMPharma\backups\logs"
$timestamp = Get-Date -Format "yyyyMMdd"

New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

$zipPath = "$targetDir\logs-$timestamp.zip"
Compress-Archive -Path "$logDir\*" -DestinationPath $zipPath -CompressionLevel Optimal

# Nettoyage après archivage
Remove-Item -Path "$logDir\*" -Exclude "*.log" -Force
```

---

## 5. Restauration

### PostgreSQL Restore

```powershell
# restore-pg.ps1
param(
    [string]$BackupFile,
    [string]$Host = "localhost",
    [string]$Db = "CHIFA_OFFICINE",
    [string]$User = "pharm"
)

Write-Warning "This will REPLACE the current database!"

$confirm = Read-Host "Are you sure? (yes/no)"
if ($confirm -ne "yes") { return }

# Stop applications
Stop-Process -Name "BMPharma.UI" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "CHIFA-OFFICINE" -Force -ErrorAction SilentlyContinue

# Restore
& pg_restore -h $Host -U $User -d $Db -v -c $BackupFile
```

### SQLite Restore

```powershell
# Restore SQLite
Stop-Process -Name "BMPharma.UI" -Force -ErrorAction SilentlyContinue
Copy-Item -Path "D:\Backups\bmpharma-20260729.db" -Destination "$env:APPDATA\BMPharma\bmpharma.db" -Force
```

### Configuration Restore

```powershell
Copy-Item -Path "D:\Backups\config\appsettings.json.20260729" -Destination "$env:ProgramFiles\BMPharma\appsettings.json" -Force
```

---

## 6. Rollback Procedure

### Rollback Version Logicielle

```powershell
# rollback.ps1
param([string]$TargetVersion)

$backupDir = "$env:ProgramData\BMPharma\updates\backup"
$installDir = "$env:ProgramFiles\BMPharma"

# Stop app
Stop-Process -Name "BMPharma.UI" -Force -ErrorAction SilentlyContinue

# Restore previous version
$versionDir = Join-Path $backupDir $TargetVersion
if (Test-Path $versionDir) {
    Remove-Item "$installDir\*" -Recurse -Force
    Copy-Item "$versionDir\*" -Recurse -Destination $installDir
    Write-Output "Rollback to $TargetVersion successful"
} else {
    Write-Error "Backup not found: $versionDir"
}
```

### Rollback PostgreSQL

```powershell
# 1. Restore last good backup
& pg_restore -h localhost -U pharm -d CHIFA_OFFICINE -c "D:\Backups\pg\CHIFA_OFFICINE-20260728.backup"

# 2. Verify
& psql -h localhost -U pharm -d CHIFA_OFFICINE -c "SELECT COUNT(*) FROM facture"
```

---

## 7. Periodic Tests

### Test Plan

| Test | Fréquence | Procédure |
|------|-----------|-----------|
| Backup integrity | Quotidienne | Vérifier taille fichier backup > 1KB |
| Restore test | Hebdomadaire | Restaurer sur instance de test, vérifier COUNT(*) |
| Rollback test | Mensuel | Simuler rollback version sur poste test |
| Log archive | Hebdomadaire | Vérifier création archive ZIP |
| Rotation cleanup | Quotidienne | Vérifier suppression backups > 7 jours |

### Automated Verification

```powershell
# verify-backups.ps1
$backupDir = "$env:ProgramData\BMPharma\backups"

# Check PG backup exists and is recent
$latestPg = Get-ChildItem "$backupDir\pg\*.backup" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($latestPg -and $latestPg.LastWriteTime -gt (Get-Date).AddDays(-1)) {
    Write-Output "✅ PG backup OK: $($latestPg.Name) ($($latestPg.Length) bytes)"
} else {
    Write-Error "❌ PG backup missing or outdated"
}

# Check SQLite backup
$latestSqlite = Get-ChildItem "$backupDir\sqlite\*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($latestSqlite -and $latestSqlite.LastWriteTime -gt (Get-Date).AddDays(-1)) {
    Write-Output "✅ SQLite backup OK: $($latestSqlite.Name)"
} else {
    Write-Error "❌ SQLite backup missing or outdated"
}
```

---

## Backup Schedule Summary

| Time | Task | Composant | Type |
|------|------|-----------|------|
| 02:00 daily | `backup-pg.ps1` | PostgreSQL | Full custom |
| 03:00 daily | `backup-sqlite.ps1` | SQLite | File copy + ZIP |
| 04:00 daily | `verify-backups.ps1` | All | Integrity check |
| 02:00 Sunday | `archive-logs.ps1` | Logs | ZIP archive |
| 03:00 Sunday | `backup-config.ps1` | Configuration | File copy |
| 04:00 Sunday | Restore test (manual or CI) | PG + SQLite | Validation |

---

## Alertes

| Condition | Action |
|-----------|--------|
| Backup size < 1KB | Email alerte + notification dashboard |
| Pas de backup depuis > 24h | Warning dashboard + log |
| Restauration échouée | Incident response (015-J) |
| Rotation non effectuée | Warning log |
