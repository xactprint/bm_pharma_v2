# 015-I — Production Monitoring

**Phase:** 015 — Pilot Readiness Package  
**Date:** 2026-07-29  
**Objective:** Définir la stratégie de monitoring en production pour le pilote.

---

## 1. Monitoring Dashboard

Le Dashboard de monitoring est intégré dans l'application BM Pharma (`ChifaDashboardViewModel`). Il est accessible depuis la sidebar après connexion.

### Indicateurs temps réel

| Métrique | Source | Intervalle | Seuil d'alerte |
|----------|--------|-----------|----------------|
| Connexion PostgreSQL | Health Check | 30s | Déconnexion > 5s |
| Connexion CHIFA | Health Check | 30s | Déconnexion > 5s |
| État synchronisation | `StatusSynchronizer` | 60s | Échec > 3 tentatives |
| Factures du jour | `CountInvoicesTodayAsync` | 60s | N/A (information) |
| Stock OK/Alert | CHIFA query | 60s | Alerte > 0 |
| Erreurs dernière heure | Log count | 300s | > 5 erreurs |
| Temps de réponse DB | Diagnostic Center | 60s | > 1s |
| Sessions actives | CHIFA query | 300s | N/A (information) |
| Build version | Assembly | Au démarrage | Version != dernière stable |

### Affichage Dashboard

```
╔══════════════════════════════════════════════════════╗
║  BM Pharma — Monitoring (auto-refresh 60s)       ║
╠══════════════════════════════════════════════════════╣
║  🔵 PostgreSQL        ● Connecté   (45ms)          ║
║  🔵 CHIFA             ● Connecté                    ║
║  🟢 Synchronisation   ● OK         08:30:15         ║
║  📊 Factures jour     ● 142                          ║
║  📦 Stock alertes     ● 3                            ║
║  ⚠️ Erreurs (1h)      ● 2                            ║
║  🖥 Build             ● 2.0.0.42                    ║
╚══════════════════════════════════════════════════════╝
```

---

## 2. Automatic Refresh

### Mécanisme

| Composant | Technologie | Paramètre |
|-----------|------------|-----------|
| Timer | `System.Timers.Timer` | Intervalle configurable |
| Intervalle par défaut | 60 secondes | `MonitoringRefreshIntervalSeconds` |
| Pause si inactif | Oui (fenêtre non visible) | `IsVisible` binding |
| Gestion erreurs | Catch + log + toast | Ne pas bloquer l'UI |
| Circuit breaker | Oui (3 échecs → 5min pause) | `Polly` policy |

### Configuration (appsettings.json)

```json
{
  "Monitoring": {
    "RefreshIntervalSeconds": 60,
    "HealthCheckTimeoutMs": 5000,
    "ErrorThreshold": 5,
    "ErrorWindowMinutes": 60,
    "CircuitBreakerAttempts": 3,
    "CircuitBreakerDurationMs": 300000
  }
}
```

---

## 3. Health Checks

### Endpoints (CLI)

```powershell
# Vérification rapide (T01-T06)
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --quick

# Vérification complète (T01-T18)
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --full

# Sortie JSON pour monitoring automatisé
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --full --json
```

### Vérifications périodiques

| Fréquence | Test | Automatique |
|-----------|------|-------------|
| 30s | T06 — PostgreSQL Connection | ✅ (Dashboard) |
| 60s | T05 — CHIFA Connection | ✅ (Dashboard) |
| 60s | T08 — Synchronization Health | ✅ (Dashboard) |
| 300s | T10 — Session Check | ✅ (Background) |
| Quotidien 04:00 | T01-T18 complet | ❌ (Manuel ou script) |

---

## 4. Alerting

### Niveaux d'alerte

| Niveau | Icône | Action | Notification |
|--------|-------|--------|-------------|
| INFO | 🔵 | Aucune | Log seulement |
| WARNING | 🟡 | Log + Dashboard | Log + UI badge |
| ERROR | 🟠 | Log + Dashboard + Notification | Log + UI + popup |
| CRITICAL | 🔴 | Tout + Alerte support | Log + UI + popup + appel support |

### Règles d'alerte

| Règle | Niveau | Condition | Action |
|-------|--------|-----------|--------|
| PostgreSQL down | CRITICAL | Health check échoue > 5s | Popup "Base de données inaccessible" |
| CHIFA déconnecté | ERROR | Health check échoue | UI warning + tentative reconnexion |
| Sync bloquée | WARNING | 3 échecs consécutifs | UI warning + bouton "Réinitialiser" |
| Erreurs fréquentes | WARNING | > 5 erreurs en 1h | Notification support |
| Circuit breaker open | ERROR | Service temporairement indisponible | UI message + timer attente |

---

## 5. Logging en Production

### Niveaux par catégorie

| Catégorie | Niveau Production | Destination |
|-----------|-------------------|-------------|
| Application | `Information` | Serilog file + Dashboard |
| Technique (.NET, Npgsql) | `Warning` | Serilog file |
| Monitoring | `Information` | Dashboard |
| Synchronisation | `Information` | Serilog file |
| Audit utilisateur | `Information` | Audit DB |

### Rétention

| Type | Durée | Rotation |
|------|-------|----------|
| Logs applicatifs | 7 jours | Taille max 50 MB |
| Logs debug | 3 jours | Activé sur demande support |
| Audit DB | 90 jours | Pas de rotation |
| Support bundle | À la demande | Supprimé après diagnostic |

---

## 6. Performance Monitoring

### Métriques collectées (Diagnostic Center)

```json
{
  "postgresql": {
    "responseTimeMs": 45,
    "connectionsActive": 3,
    "databaseSizeMB": 128
  },
  "application": {
    "memoryMB": 64,
    "uptime": "12h 34m",
    "version": "2.0.0.42"
  },
  "synchronization": {
    "lastSync": "2026-07-29T08:30:15",
    "status": "OK",
    "errorsLastHour": 2
  }
}
```

### Seuils de performance

| Métrique | OK | Warning | Critical |
|----------|-----|---------|----------|
| Temps réponse DB | < 200ms | 200ms–1s | > 1s |
| Mémoire application | < 128 MB | 128–256 MB | > 256 MB |
| Taille base de données | < 500 MB | 500 MB–1 GB | > 1 GB |
| Connexions actives | < 5 | 5–10 | > 10 |
| Temps disponibilité | > 99% | 95–99% | < 95% |

---

## 7. Runbook Monitoring

### Consultation quotidienne

```powershell
# 1. Vérifier le Dashboard (60s auto-refresh)
# 2. Vérifier les logs récents
Get-Content "$env:ProgramData\BMPharma\logs\bmpharma-current.log" -Tail 50

# 3. Générer un rapport de santé
& "C:\Program Files\BMPharma\BMPharma.Diagnostic.exe" --quick --json
```

### Actions correctives

| Problème | Action |
|----------|--------|
| Dashboard ne charge pas | Vérifier `ChifaIntegrationFacade`, circuit breaker, logs |
| Métriques à zéro | Vérifier `StatusSynchronizer`, `CountInvoicesTodayAsync` |
| Refresh ne fonctionne pas | Vérifier Timer, `IsVisible` binding, exceptions silencieuses |
| Erreurs en augmentation | Générer support bundle, analyser logs, contacter support |
