# 015-B — Diagnostic Center

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir le Diagnostic Center — outil de vérification automatique de l'environnement.

---

## Architecture

Le Diagnostic Center est une vue dédiée dans BM Pharma (accessible depuis la sidebar) ou un outil CLI séparé (`BMPharma.Diagnostic.exe`).

```
BMPharma.Diagnostic.exe [--json] [--quick] [--full]
```

### Modes

| Mode | Durée | Tests | Usage |
|------|-------|-------|-------|
| `--quick` | < 5s | 1-6 | Vérification rapide avant utilisation |
| `--full` | < 30s | 1-18 | Diagnostic complet avant déploiement |
| `--json` | — | Tous | Sortie JSON pour monitoring automatisé |

---

## Test List

### T01 — Windows Compatible

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que le système d'exploitation est compatible |
| **Méthode** | `Environment.OSVersion.Version` ≥ 10.0 (Windows 10) + `Environment.Is64BitProcess` |
| **Résultat attendu** | Windows 10+ 64-bit |
| **Action corrective** | Mettre à jour Windows ou installer sur un poste compatible |
| **Priorité** | BLOCKING |

### T02 — .NET Runtime

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que le .NET 8 Runtime est installé |
| **Méthode** | Vérifier `dotnet --list-runtimes` contient `Microsoft.NETCore.App 8.0.x` |
| **Résultat attendu** | .NET 8.0.x présent |
| **Action corrective** | Installer .NET 8 Runtime depuis https://dotnet.microsoft.com |
| **Priorité** | BLOCKING |

### T03 — PostgreSQL Available

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que le serveur PostgreSQL est joignable |
| **Méthode** | `Test-NetConnection -ComputerName <host> -Port 5432` |
| **Résultat attendu** | TCP port 5432 ouvert |
| **Action corrective** | Vérifier le service PostgreSQL, firewall, connectivité réseau |
| **Priorité** | BLOCKING |

### T04 — PostgreSQL Accessible

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que l'authentification PostgreSQL fonctionne |
| **Méthode** | `SELECT 1` via Npgsql avec ConnectionString configurée |
| **Résultat attendu** | `1` retourné sans erreur |
| **Action corrective** | Vérifier credentials, `pg_hba.conf`, utilisateur `pharm` |
| **Priorité** | BLOCKING |

### T05 — Tables Présentes

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que les tables CHIFA_OFFICINE sont présentes |
| **Méthode** | `SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'` |
| **Résultat attendu** | Tables `facture`, `detail_fact`, `bordereau`, `medicament`, `parametre`, `signature` présentes |
| **Action corrective** | Vérifier le schéma, contacter DBA |
| **Priorité** | HIGH |

### T06 — CHIFA Installé

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que CHIFA-OFFICINE est installé |
| **Méthode** | `Test-Path -Path <ApplicationPath>` (depuis config) |
| **Résultat attendu** | Chemin CHIFA-OFFICINE existe |
| **Action corrective** | Installer CHIFA-OFFICINE ou corriger `ApplicationPath` |
| **Priorité** | HIGH |

### T07 — CHIFA Démarré

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que CHIFA-OFFICINE est en cours d'exécution |
| **Méthode** | `Get-Process -Name "CHIFA-OFFICINE" -ErrorAction SilentlyContinue` |
| **Résultat attendu** | Processus trouvé |
| **Action corrective** | Démarrer CHIFA-OFFICINE |
| **Priorité** | HIGH |

### T08 — SAM / Token Détecté

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que le token PKCS#11 est présent |
| **Méthode** | `IChifaTokenService.GetTokenStatusAsync()` |
| **Résultat attendu** | `isPresent = true` |
| **Action corrective** | Insérer le token dans le lecteur |
| **Priorité** | MEDIUM |

### T09 — Certificats Présents

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que les certificats CHIFA sont installés |
| **Méthode** | Vérifier `Cert:\CurrentUser\My` pour certificats CHIFA |
| **Résultat attendu** | Au moins un certificat CHIFA valide |
| **Action corrective** | Installer le certificat CHIFA |
| **Priorité** | MEDIUM |

### T10 — Permissions Windows

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que l'utilisateur a les permissions nécessaires |
| **Méthode** | Test écriture dans `%APPDATA%\BMPharma`, `%PROGRAMDATA%\BMPharma`, lecture `ProgramFiles\BMPharma` |
| **Résultat attendu** | Écriture/lecture OK |
| **Action corrective** | Exécuter en tant qu'administrateur ou ajuster les permissions |
| **Priorité** | HIGH |

### T11 — Répertoires BM Pharma

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que les répertoires de l'application existent |
| **Méthode** | `Test-Path` pour `InstallDir`, `AppData`, `Logs`, `Config` |
| **Résultat attendu** | Tous les répertoires existent avec les bons droits |
| **Action corrective** | Créer les répertoires manquants |
| **Priorité** | MEDIUM |

### T12 — Logs

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que les logs s'écrivent correctement |
| **Méthode** | Écrire un log test → vérifier fichier créé et lisible |
| **Résultat attendu** | Fichier log créé, contenu valide |
| **Action corrective** | Vérifier permissions dossier logs, configuration Serilog |
| **Priorité** | MEDIUM |

### T13 — Connexion Réseau

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie la connectivité réseau globale |
| **Méthode** | Ping serveur PostgreSQL + résolution DNS |
| **Résultat attendu** | Ping succès, DNS résolu |
| **Action corrective** | Vérifier réseau, firewall, DNS |
| **Priorité** | HIGH |

### T14 — SQLite Database

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que la base locale BM Pharma est accessible |
| **Méthode** | Ouvrir `bmpharma.db` avec SQLite, exécuter `SELECT 1` |
| **Résultat attendu** | Base accessible, tables `Invoice`, `InvoiceLine`, etc. présentes |
| **Action corrective** | Réinitialiser la base locale, contacter support |
| **Priorité** | MEDIUM |

### T15 — Configuration File

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie que `appsettings.json` est valide |
| **Méthode** | Parse JSON, vérifier sections `CHIFA`, `ConnectionStrings`, `Serilog` |
| **Résultat attendu** | JSON valide, sections requises présentes |
| **Action corrective** | Restaurer le fichier de configuration par défaut |
| **Priorité** | BLOCKING |

### T16 — Circuit Breaker Status

| Champ | Valeur |
|-------|--------|
| **Description** | Vérifie l'état du Circuit Breaker |
| **Méthode** | `ChifaCircuitBreaker.GetState("ChifaIntegration")` |
| **Résultat attendu** | `Closed` |
| **Action corrective** | Attendre 30s ou réinitialiser si ouvert |
| **Priorité** | MEDIUM |

### T17 — Mode d'Intégration

| Champ | Valeur |
|-------|--------|
| **Description** | Affiche le mode d'intégration actuel |
| **Méthode** | `ChifaIntegrationModeProvider.CurrentMode` |
| **Résultat attendu** | ReadOnly (Test/Production si configuré) |
| **Action corrective** | Vérifier configuration `Mode` dans appsettings.json |
| **Priorité** | INFO |

### T18 — Version Application

| Champ | Valeur |
|-------|--------|
| **Description** | Affiche la version installée |
| **Méthode** | `Assembly.GetExecutingAssembly().GetName().Version` |
| **Résultat attendu** | Version correspondant au MSI installé |
| **Action corrective** | Mettre à jour si version obsolète |
| **Priorité** | INFO |

---

## Output Format

### Console (default)

```
═══════════════════════════════════════════════════
 BM Pharma Diagnostic Center v1.0
═══════════════════════════════════════════════════

Tests: quick (6) / full (18)

[T01] Windows Compatible       ✅ Windows 10 64-bit
[T02] .NET Runtime              ✅ 8.0.6
[T03] PostgreSQL Available      ✅ host:5432
[T04] PostgreSQL Accessible     ✅ pharm@CHIFA_OFFICINE
[T05] Tables Présentes          ✅ 6/6
[T06] CHIFA Installé            ✅ C:\Program Files\CHIFA-OFFICINE
[T07] CHIFA Démarré             ⚠ Process not found
[T08] SAM Détecté               ✅ Token présent
[T09] Certificats Présents      ✅ CHIFA cert valid
...
═══════════════════════════════════════════════════
BLOCKING: 0   HIGH: 0   MEDIUM: 1   INFO: 0   PASS: 16
═══════════════════════════════════════════════════
Status: ✅ READY (minor warnings)
```

### JSON (`--json`)

```json
{
  "version": "2.0.0",
  "timestamp": "2026-07-29T18:00:00Z",
  "tests": [
    {
      "id": "T01",
      "name": "Windows Compatible",
      "status": "pass",
      "severity": "blocking",
      "message": "Windows 10 64-bit",
      "durationMs": 5
    }
  ],
  "summary": {
    "total": 18,
    "pass": 16,
    "warning": 1,
    "fail": 0,
    "blocking": 0
  },
  "status": "ready"
}
```

---

## Integration

### From BM Pharma UI

- Vue dédiée `DiagnosticView.xaml` + `DiagnosticViewModel.cs`
- Lancement automatique au premier démarrage
- Bouton "Diagnostic" dans le dashboard
- Résumé permanent dans le monitoring

### From CLI

```powershell
# Quick check
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --quick

# Full diagnostic with JSON output
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --full --json > diagnostic.json
```

### From Support Bundle

Le Diagnostic Center est inclus dans le Support Bundle (015-F) pour génération automatique.
