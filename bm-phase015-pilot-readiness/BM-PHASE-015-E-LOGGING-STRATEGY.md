# 015-E — Logging Strategy

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir la stratégie complète de logging.

---

## Log Categories

| Category | Source | Niveau Production | Niveau Debug | Destination |
|----------|--------|-------------------|--------------|-------------|
| Application | BM Pharma code | `Information` | `Debug` | Serilog file |
| Technique | .NET / Npgsql | `Warning` | `Information` | Serilog file |
| EF Core | `Microsoft.EntityFrameworkCore` | `Warning` | `Information` | Serilog file |
| PostgreSQL | Npgsql logging | `Warning` | `Information` | Serilog file |
| Monitoring | `ChifaMonitoringService` | `Information` | `Debug` | Serilog file + dashboard |
| Synchronisation | Synchonizers | `Information` | `Debug` | Serilog file |
| Utilisateur | UI actions | `Information` | `Information` | Serilog file + audit DB |

---

## 1. Application Logs

### Source
- `ILogger<T>` injections dans tous les services et ViewModels
- `Microsoft.Extensions.Logging` abstrait → Serilog sink

### Format
```
2026-07-29 08:30:15.123 +01:00 [INF] (CorrelationId: a1b2c3d4e5f6) Connection established to PostgreSQL
2026-07-29 08:30:15.456 +01:00 [DBG] (CID: a1b2c3d4e5f6) Health check completed in 42ms
2026-07-29 08:30:20.789 +01:00 [WRN] (CID: a1b2c3d4e5f6) Circuit breaker state: Open (key=ChifaIntegration)
2026-07-29 08:30:25.012 +01:00 [ERR] (CID: a1b2c3d4e5f6) Invoice preparation failed: Timeout expired
```

### Champs structurés
| Champ | Exemple | Source |
|-------|---------|--------|
| Timestamp | `2026-07-29T08:30:15.123Z` | Serilog |
| Level | `INF`, `DBG`, `WRN`, `ERR`, `FTL` | Niveau |
| CorrelationId | `a1b2c3d4e5f6` | `CorrelationContext.Current` |
| SourceContext | `BMPharma.CHIFA.Services.ChifaIntegrationFacade` | Logger category |
| Message | Texte structuré | Log call |
| Exception | `ToString()` | Si exception |
| DurationMs | `42` | Si mesuré |
| Operation | `HealthCheck` | Si opération nommée |

---

## 2. Technical Logs

### Source
- .NET runtime
- Npgsql (ADO.NET)
- CommunityToolkit.Mvvm

### Configuration (Production)
```json
"Microsoft": "Warning",
"Microsoft.EntityFrameworkCore": "Warning",
"System": "Warning",
"Npgsql": "Warning"
```

### Configuration (Debug)
```json
"Microsoft": "Information",
"Microsoft.EntityFrameworkCore": "Information",
"Npgsql": "Information"
```

### Utilité
- Détection des requêtes SQL lentes (EF Core)
- Détection des échecs de pooling Npgsql
- Détection des exceptions non gérées

---

## 3. EF Core Logs

### Source
- `Microsoft.EntityFrameworkCore.Database.Command`
- Requêtes SQL générées par EF Core

### Production
```json
"Microsoft.EntityFrameworkCore": "Warning"
```
Uniquement les commandes en échec.

### Debug
```json
"Microsoft.EntityFrameworkCore": "Information"
```
Toutes les commandes SQL avec durée.

### Exemple
```
2026-07-29 08:30:15.456 [INF] Executed DbCommand (42ms) [Parameters=[@__p_0='?' (DbType = String)], CommandType='Text', CommandTimeout='30']
SELECT f.* FROM facture f WHERE f.num_fact = @__p_0 LIMIT 2
```

---

## 4. PostgreSQL Logs

### Source
- Npgsql logging
- PostgreSQL serveur (`postgresql.conf`)

### Configuration PostgreSQL côté serveur
```ini
log_min_duration_statement = 1000    # Log queries > 1s
log_checkpoints = on
log_connections = on
log_disconnections = on
log_lock_waits = on
```

### Utilité
- Détection des requêtes lentes côté serveur
- Détection des deadlocks
- Détection des connexions anormales

---

## 5. Monitoring Logs

### Source
- `ChifaMonitoringService`
- `ChifaMetricsService`
- `ChifaHealthCheckService`
- `ChifaCircuitBreaker`

### Événements loggés
| Événement | Niveau | Fréquence |
|-----------|--------|-----------|
| Health check succès | `Debug` | 60s |
| Health check échec | `Warning` | 60s |
| Circuit Breaker ouvert | `Warning` | Sur changement |
| Circuit Breaker fermé | `Information` | Sur changement |
| Métrique enregistrée | `Debug` | Par opération |
| Seuil dépassé | `Warning` | Sur dépassement |

---

## 6. Synchronisation Logs

### Source
- `InvoiceSynchronizer`
- `BordereauSynchronizer`
- `ChifaIntegrationFacade.SynchronizeAsync()`

### Événements loggés
| Événement | Niveau |
|-----------|--------|
| Sync démarrée | `Information` |
| Sync réussie | `Information` (avec durée + nb éléments) |
| Sync échouée | `Error` (avec détails) |
| CHIFA indisponible | `Warning` |
| Aucune donnée à sync | `Debug` |

---

## 7. User Action Logs

### Source
- `ChifaAuditService` (audit DB)
- ViewModels (ILogger)

### Audit DB (persistant)
| Opération | Table audit |
|-----------|-------------|
| Préparation facture | `ChifaAuditLog` |
| Validation facture | `ChifaAuditLog` |
| Création bordereau | `BordereauAuditLog` |
| Signature | `BordereauAuditLog` |
| Clôture | `BordereauAuditLog` |
| Transmission | `BordereauAuditLog` |
| Erreur | `ChifaAuditLog` |

### UI Logger (fichier)
```csharp
_logger.LogInformation("User {User} validated invoice {NumFact}", user, numFact);
```

---

## 8. Retention Policy

| Log Type | Rétention Production | Rétention Debug | Compression | Rotation |
|----------|---------------------|-----------------|-------------|----------|
| Application | 30 jours | 7 jours | Non (daily rotation) | Quotidienne |
| EF Core | 30 jours | 7 jours | Non (inclus app) | Quotidienne |
| Monitoring | 30 jours | 7 jours | Non (inclus app) | Quotidienne |
| PostgreSQL (serveur) | 7 jours | 7 jours | Oui (logrotate) | Horaire |
| Audit DB | Permanent | Permanent | N/A | N/A |

### Configuration Serilog
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "%APPDATA%\\BMPharma\\logs\\bmpharma-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] (CID: {CorrelationId}) {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

---

## 9. Log Levels Per Environment

### Production
```
Default:         Information
Microsoft:       Warning
EF Core:         Warning
Npgsql:          Warning
System:          Warning
```

### Debug / Development
```
Default:         Debug
Microsoft:       Information
EF Core:         Information
Npgsql:          Information
System:          Warning
```

### Diagnostic / Trace
```
Default:         Verbose
Microsoft:       Information
EF Core:         Debug
Npgsql:          Debug
System:          Warning
```

---

## 10. Sensitive Data Protection

### Données à NE PAS logger
- ❌ Mots de passe (connection string)
- ❌ Token PKCS#11
- ❌ Données patient identifiantes (noms, adresses)
- ❌ Numéros de sécurité sociale complets

### Données acceptables
- ✅ N° facture (numéro séquentiel, non nominatif)
- ✅ Code CIP (produit, pas patient)
- ✅ Montants (pas de données personnelles)
- ✅ ID de corrélation
- ✅ Noms d'opérations

### Règle
```csharp
// BON : pas de données patient
_logger.LogInformation("Invoice {NumFact} prepared in {Duration}ms", numFact, sw.ElapsedMilliseconds);

// MAUVAIS : ne pas logger
_logger.LogInformation("Patient {Name} invoice {NumFact}", patientName, numFact);
```
