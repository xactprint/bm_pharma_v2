# 014-G — Administrator Guide

**Phase:** 014 — User Acceptance Validation  
**Date:** 2026-07-29  
**Audience:** Administrateur système / DBA  
**Version logiciel:** BM Pharma v2.0  

---

## 1. Architecture Système

```
┌─────────────────────────────────────────────────────────────────┐
│                    Pharmacie (Poste Windows)                     │
│                                                                   │
│  ┌──────────────┐    ┌──────────────────┐    ┌──────────────┐   │
│  │              │    │                  │    │              │   │
│  │  BM Pharma   │────│  PostgreSQL 9.3  │────│ CHIFA-OFFICINE│   │
│  │  (WPF .NET8) │    │  CHIFA_OFFICINE  │    │  (Application) │   │
│  │              │    │                  │    │              │   │
│  └──────────────┘    └──────────────────┘    └──────────────┘   │
│         │                                                         │
│         │ SQLite (locale)                                         │
│         ▼                                                         │
│  ┌──────────────┐                                                 │
│  │  BM Pharma   │                                                 │
│  │  Data (local)│                                                 │
│  └──────────────┘                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Composants
| Composant | Technologie | Rôle |
|-----------|------------|------|
| BM Pharma | WPF .NET 8 | Application métier |
| PostgreSQL 9.3.4 | Base de données | CHIFA_OFFICINE (lecture + écriture) |
| SQLite | Base de fichiers | Données locales BM Pharma |
| CHIFA-OFFICINE | Application externe | Éditeur officiel CHIFA |
| SAM / PKCS#11 | Carte à puce | Signature électronique |

---

## 2. Sauvegarde

### Base PostgreSQL CHIFA_OFFICINE

```bash
# Sauvegarde complète
pg_dump -h localhost -U pharm -F c -b -v -f "CHIFA_OFFICINE_$(date +%Y%m%d).backup" CHIFA_OFFICINE

# Sauvegarde sans données (schéma seul)
pg_dump -h localhost -U pharm -s -f "CHIFA_OFFICINE_schema.sql" CHIFA_OFFICINE
```

### Base SQLite BM Pharma

```bash
# Copie simple (fichier unique)
copy "%APPDATA%\BMPharma\bmpharma.db" "D:\Backups\BMPharma_$(date +%Y%m%d).db"

# Localisation du fichier (selon configuration)
%APPDATA%\BMPharma\bmpharma.db
./bmpharma.db  (dossier d'installation)
```

### Fichiers de configuration
```
appsettings.json              → Configuration principale
appsettings.Development.json  → Surcharge développement
logs/bmpharma-.log            → Logs d'application
```

---

## 3. Restauration

### Restaurer PostgreSQL

```bash
pg_restore -h localhost -U pharm -d CHIFA_OFFICINE -v "CHIFA_OFFICINE_20260729.backup"
```

> **⚠ Important :** La restauration PostgreSQL nécessite que l'application BM Pharma soit fermée et que CHIFA-OFFICINE soit arrêté.

### Restaurer SQLite

```bash
copy "D:\Backups\BMPharma_20260729.db" "%APPDATA%\BMPharma\bmpharma.db"
```

---

## 4. Logs

### Localisation
```
%APPDATA%\BMPharma\logs\
OU
./logs/  (dossier d'installation)
```

### Format (Serilog)
```
2026-07-29 08:30:15.123 +01:00 [INF] Connection established to PostgreSQL
2026-07-29 08:30:15.456 +01:00 [DBG] Health check completed in 42ms
2026-07-29 08:30:20.789 +01:00 [WRN] Circuit breaker state: Open (key=ChifaIntegration)
2026-07-29 08:30:25.012 +01:00 [ERR] Invoice preparation failed: Timeout expired
```

### Niveaux de Log
| Niveau | Utilisation |
|--------|-------------|
| `Verbose` | Données de debug détaillées |
| `Debug` | Informations de développement |
| `Information` | Opérations normales (connexion, déconnexion, validation) |
| `Warning` | Anomalies non bloquantes (CB ouvert, timeout, échec unique) |
| `Error` | Erreurs bloquantes (échec écriture, exception non gérée) |
| `Fatal` | Erreur critique (arrêt application) |

### Rotation
- Quotidienne
- Conservation : 30 jours (configurable)
- Taille max par fichier : 10 Mo

---

## 5. Monitoring

### Tableau de bord administrateur

Le dashboard BM Pharma expose en temps réel :

| Métrique | Source | Format |
|----------|--------|--------|
| État PostgreSQL | `ChifaHealthCheckService` | Vert/Orange/Rouge |
| État CHIFA | `IChifaIntegrationService` | Vert/Rouge |
| État token | `IChifaTokenService` | Présent/Absent |
| État signature | `IChifaSigningService` | Signé/Non signé/Erreur |
| Circuit Breaker | `ChifaCircuitBreaker` | Closed/HalfOpen/Open |
| Métriques opérations | `ChifaMetricsService` | Total/Succès/Échecs/Temps moyen |
| ID corrélation | `CorrelationContext` | Chaîne 12 car. |
| Synchronisation | `ChifaSyncSummary` | Factures/Bordereaux/Erreurs |

### Vérification rapide (health check)

```bash
# Tester la connexion PostgreSQL
psql -h localhost -U pharm -d CHIFA_OFFICINE -c "SELECT 1"

# Vérifier les logs BM Pharma en temps réel
Get-Content -Path "./logs/bmpharma-.log" -Tail 50 -Wait
```

---

## 6. Diagnostic

### Vérification de la Configuration

```bash
# Tester la connexion PostgreSQL depuis le poste client
Test-NetConnection -ComputerName localhost -Port 5432

# Vérifier que CHIFA-OFFICINE est accessible
Test-Path -Path "C:\Program Files\CHIFA-OFFICINE\CHIFA-OFFICINE.exe"

# Vérifier que l'application peut démarrer
& "C:\Program Files\BMPharma\BMPharma.UI.exe" --diagnostic
```

### Problèmes Courants

| Symptôme | Diagnostic | Solution |
|----------|-----------|----------|
| "PostgreSQL indisponible" | Ping + port 5432 | Vérifier service PG et firewall |
| "CHIFA hors ligne" | Vérifier processus CHIFA | Démarrer CHIFA-OFFICINE |
| "Token absent" | Vérifier lecteur + token | Insérer token PKCS#11 |
| "Circuit Breaker open" | Logs d'erreurs récentes | Attendre 30s ou réinitialiser |
| "Erreur de connexion" | Logs d'exception Npgsql | Vérifier ConnectionString |

### Réinitialisation du Circuit Breaker

```bash
# Via redémarrage BM Pharma
# OU (modification directe déconseillée)
# Attendre le timeout automatique (30s)
```

---

## 7. Mode ReadOnly

### Comportement
- **Par défaut :** L'application démarre en mode ReadOnly
- **Protection :** 3 couches (mode provider → WriteGuard → facade)
- **Affichage :** Bandeau orange "MODE LECTURE SEULE"
- **Opérations :** Validation et soumission simulées (aucune écriture PG)

### Activation / Désactivation

Dans `appsettings.json` :
```json
{
  "CHIFA": {
    "Mode": "ReadOnly"     // ReadOnly | Test | Production
  }
}
```

| Mode | Écriture PG | Simulation | Usage |
|------|------------|------------|-------|
| `ReadOnly` | ❌ Bloquée | ✅ Messages simulés | Découverte, test, démo |
| `Test` | ✅ Réelle | ❌ Aucune | Test d'intégration |
| `Production` | ✅ Réelle | ❌ Aucune | Utilisation quotidienne |

---

## 8. Mode Production

### Prérequis
- [ ] Base PostgreSQL CHIFA_OFFICINE accessible
- [ ] Utilisateur PostgreSQL avec droits INSERT/UPDATE
- [ ] CHIFA-OFFICINE démarré et configuré
- [ ] Token PKCS#11 opérationnel
- [ ] Sauvegarde récente effectuée
- [ ] Logs vérifiés (aucune erreur persistante)

### Activation
1. Modifier `appsettings.json` : `"Mode": "Production"`
2. Redémarrer l'application
3. Vérifier que le bandeau orange "ReadOnly" a disparu
4. Effectuer une facture test avec vérification dans CHIFA-OFFICINE

### Surveillance
- Vérifier le Circuit Breaker régulièrement
- Surveiller les logs d'erreur
- Valider les écritures dans CHIFA-OFFICINE
- Maintenir une sauvegarde quotidienne

---

## 9. Résolution de Problèmes Avancée

### Problème : Conflit de version EF Core / Npgsql avec PG 9.3.4

**Symptôme :** `NpgsqlException` avec message de type "syntax error" ou "type not found"

**Solution :**
1. Vérifier la version de Npgsql (compatible PG 9.3.4)
2. Vérifier les types de colonnes (pas de type `jsonb`, `uuid` non supportés en 9.3)
3. Vérifier `DateTime.Kind` = `Unspecified` pour `timestamp without time zone`

### Problème : `DateTime Kind` mismatch

**Symptôme :** Erreur EF Core lors de l'écriture de `date_fact`

**Solution :**
```csharp
// Forcer Unspecified pour timestamp without time zone
entity.DateFact = DateTime.SpecifyKind(entity.DateFact, DateTimeKind.Unspecified);
```

### Problème : Rollback impossible

**Symptôme :** Workflow state = `RollbackRequired`

**Procédure :**
1. Identifier la facture problématique (N° facture, ID corrélation)
2. Vérifier si la facture a été écrite dans PostgreSQL
3. Si écrite : supprimer manuellement la ligne dans `facture` + `detail_fact`
4. Marquer la facture comme annulée dans BM Pharma
5. Recontacter le support BM Pharma

---

## 10. Références Techniques

| Documentation | Emplacement |
|--------------|-------------|
| Architecture CHIFA | `BM_PHARMA_CHIFA_INTEGRATION_ARCHITECTURE.md` |
| Contrat base données | `BM_PHARMA_CHIFA_DATABASE_CONTRACT.md` |
| Contrat facture | `BM_PHARMA_CHIFA_INVOICE_CONTRACT.md` |
| Contrat bordereau | `BM_PHARMA_CHIFA_BORDEREAU_CONTRACT.md` |
| State machine | `BM_PHARMA_CHIFA_STATE_MACHINE.md` |
| Test plan | `BM_PHARMA_CHIFA_TEST_PLAN.md` |
| Sécurité | `BM_PHARMA_CHIFA_SECURITY.md` |
| Phase 012 (audit final) | `bm-phase012-end-to-end-validation/` |
| Phase 013 (UI) | `BM-PHASE-013-UI-INTEGRATION.md` |
