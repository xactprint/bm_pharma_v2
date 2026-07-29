# 015-C — Configuration Manager

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir le contrat de configuration pour tous les niveaux.

---

## Configuration Layers

```
appsettings.json          (base — install folder)
└── appsettings.Development.json  (dev override — NOT in production)
    └── ProgramData\BMPharma\config\pharmacie.json  (pharmacy override)
        └── Environment variables  (runtime override)
            └── UI Settings  (user preferences)
```

### Priority (highest wins)

1. UI Settings (utilisateur, persisté dans SQLite)
2. Environment variables (`CHIFA__ConnectionString`, `CHIFA__Mode`, etc.)
3. `ProgramData\BMPharma\config\pharmacie.json`
4. `appsettings.Development.json` (ignoré en production)
5. `appsettings.json` (base)

---

## 1. Application Base (`appsettings.json`)

```json
{
  "CHIFA": {
    "ConnectionString": "Host=localhost;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=***",
    "Mode": "ReadOnly",
    "Schema": "public",
    "ApplicationPath": "C:\\Program Files\\CHIFA-OFFICINE\\"
  },
  "ConnectionStrings": {
    "BmPharmaDb": "Data Source=%APPDATA%\\BMPharma\\bmpharma.db"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "%APPDATA%\\BMPharma\\logs\\bmpharma-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760
        }
      }
    ]
  },
  "BMPharma": {
    "Diagnostic": {
      "AutoRunOnStartup": true,
      "QuickCheckOnly": true
    },
    "Updates": {
      "CheckUrl": "",
      "CheckIntervalHours": 24,
      "AutoDownload": false
    },
    "Backup": {
      "Enabled": true,
      "IntervalHours": 24,
      "RetentionDays": 7,
      "TargetPath": "%PROGRAMDATA%\\BMPharma\\backups\\"
    },
    "Monitoring": {
      "HealthCheckIntervalMs": 60000,
      "AutoRefreshIntervalMs": 30000,
      "CircuitBreakerFailureThreshold": 3,
      "CircuitBreakerOpenTimeoutMs": 30000,
      "CircuitBreakerSuccessThreshold": 2
    }
  }
}
```

---

## 2. User Configuration (`appsettings.Development.json`)

**Usage :** Développement uniquement. Ignoré si `BMPharma.UI.exe` est en mode Release.

```json
{
  "CHIFA": {
    "Mode": "Test",
    "ConnectionString": "Host=localhost;Port=5432;Database=CHIFA_OFFICINE_TEST;Username=pharm;Password=dev"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

---

## 3. Machine Configuration (`pharmacie.json`)

**Location :** `%ProgramData%\BMPharma\config\pharmacie.json`  
**Usage :** Sur-couche machine. Permet à l'administrateur de centraliser la configuration.

```json
{
  "CHIFA": {
    "ConnectionString": "Host=srv-pharma;Port=5432;Database=CHIFA_OFFICINE;Username=pharm;Password=***",
    "Mode": "ReadOnly",
    "ApplicationPath": "D:\\CHIFA-OFFICINE\\"
  },
  "BMPharma": {
    "Backup": {
      "TargetPath": "D:\\Backups\\BMPharma\\"
    }
  }
}
```

---

## 4. Pharmacy Configuration

**Définition :** Ensemble des paramètres propres à la pharmacie, stockés dans `pharmacie.json` et/ou saisis dans l'UI.

| Paramètre | Source | Description |
|-----------|--------|-------------|
| Code centre | Formulaire facture | Code de la pharmacie (ex: 11600) |
| Nom pharmacie | UI Settings | Nom d'affichage |
| Adresse | UI Settings | Adresse postale |
| N° agrément | UI Settings | Numéro d'agrément CNAS |

---

## 5. CHIFA Configuration

**Définition :** Paramètres de connexion au système CHIFA-OFFICINE.

| Paramètre | Clé JSON | Source |
|-----------|----------|--------|
| Connection string | `CHIFA.ConnectionString` | `appsettings.json` → `pharmacie.json` |
| Mode | `CHIFA.Mode` | `appsettings.json` → ENV |
| Schéma | `CHIFA.Schema` | `appsettings.json` (par défaut: public) |
| Chemin CHIFA | `CHIFA.ApplicationPath` | `appsettings.json` |

---

## 6. PostgreSQL Configuration

**Définition :** Paramètres de connexion PostgreSQL (inclus dans la connection string).

| Paramètre | Dans ConnectionString | Exemple |
|-----------|----------------------|---------|
| Host | `Host` | `Host=srv-pharma` |
| Port | `Port` | `Port=5432` |
| Database | `Database` | `Database=CHIFA_OFFICINE` |
| Username | `Username` | `Username=pharm` |
| Password | `Password` | `Password=***` |
| Pooling | `Pooling` | `Pooling=true` |
| Timeout | `Timeout` | `Timeout=15` |
| CommandTimeout | `Command Timeout` | `Command Timeout=30` |

---

## 7. Logging Configuration

| Paramètre | Clé JSON | Default | Description |
|-----------|----------|---------|-------------|
| Default level | `Serilog.MinimumLevel.Default` | `Information` | `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal` |
| Microsoft level | `Serilog.MinimumLevel.Override.Microsoft` | `Warning` | Réduit le bruit des logs framework |
| EF Core level | `Serilog.MinimumLevel.Override.Microsoft.EntityFrameworkCore` | `Warning` | Requêtes SQL en Debug |
| Log path | `Serilog.WriteTo[0].Args.path` | `%APPDATA%\logs\bmpharma-.log` | Template avec date |
| Rolling interval | `Serilog.WriteTo[0].Args.rollingInterval` | `Day` | `Day`, `Hour`, `Month` |
| Retention | `Serilog.WriteTo[0].Args.retainedFileCountLimit` | `30` | Nombre de fichiers conservés |
| Max file size | `Serilog.WriteTo[0].Args.fileSizeLimitBytes` | `10485760` (10MB) | Taille max par fichier |

---

## 8. Monitoring Configuration

| Paramètre | Clé JSON | Default | Description |
|-----------|----------|---------|-------------|
| Health check interval | `BMPharma.Monitoring.HealthCheckIntervalMs` | 60000 | Intervalle du health check automatique (ms) |
| Dashboard refresh | `BMPharma.Monitoring.AutoRefreshIntervalMs` | 30000 | Intervalle du refresh dashboard (ms) |
| CB failure threshold | `BMPharma.Monitoring.CircuitBreakerFailureThreshold` | 3 | Nombre d'échecs avant ouverture |
| CB open timeout | `BMPharma.Monitoring.CircuitBreakerOpenTimeoutMs` | 30000 | Durée avant HalfOpen (ms) |
| CB success threshold | `BMPharma.Monitoring.CircuitBreakerSuccessThreshold` | 2 | Succès pour fermeture |

---

## Configuration Validation

```csharp
public class ConfigurationValidator
{
    public List<string> Validate(IConfiguration config)
    {
        var errors = new List<string>();
        
        // CHIFA section required
        var chifaSection = config.GetSection("CHIFA");
        if (!chifaSection.Exists())
            errors.Add("CHIFA section is required");
            
        // ConnectionString required
        var connStr = chifaSection["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
            errors.Add("CHIFA:ConnectionString is required");
            
        // Mode must be valid
        var mode = chifaSection["Mode"];
        if (!new[] { "ReadOnly", "Test", "Production" }.Contains(mode))
            errors.Add($"CHIFA:Mode must be ReadOnly, Test, or Production (got: {mode})");
            
        // Schema defaults to public
        // ApplicationPath is optional (for CHIFA integration)
        
        return errors;
    }
}
```

---

## Configuration Manager — UI

Future vue de configuration dans BM Pharma :

```
Configuration BM Pharma
─────────────────────────────────────────────
Mode d'intégration : [ReadOnly ▼]
Hôte PostgreSQL    : [srv-pharma        ]
Port               : [5432              ]
Base de données    : [CHIFA_OFFICINE    ]
Utilisateur        : [pharm             ]
Mot de passe       : [********          ]
Chemin CHIFA       : [C:\Program Files\CHIFA-OFFICINE]
─────────────────────────────────────────────
[Sauvegarder] [Tester la connexion] [Diagnostic]
```
