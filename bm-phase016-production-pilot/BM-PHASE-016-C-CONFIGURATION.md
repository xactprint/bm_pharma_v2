# 016-C — Production Configuration Validation

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Valider la configuration complète de l'application.

---

## Environnement

| Champ | Valeur |
|-------|--------|
| **Auteur test** | |
| **Date** | |
| **Fichier config** | |

---

## 1. appsettings.json

### 1.1 Structure

| # | Champ | Attendu | Résultat |
|---|-------|---------|----------|
| 1.1 | `ConnectionStrings.PostgreSQL` | Présent, valide | ☐ OK / ☐ KO |
| 1.2 | `ConnectionStrings.CHIFA` | Présent, valide | ☐ OK / ☐ KO |
| 1.3 | `Monitoring.RefreshIntervalSeconds` | 60 | ☐ OK / ☐ KO |
| 1.4 | `Monitoring.HealthCheckTimeoutMs` | 5000 | ☐ OK / ☐ KO |
| 1.5 | `Monitoring.ErrorThreshold` | 5 | ☐ OK / ☐ KO |
| 1.6 | `Monitoring.CircuitBreakerAttempts` | 3 | ☐ OK / ☐ KO |
| 1.7 | `Monitoring.CircuitBreakerDurationMs` | 300000 | ☐ OK / ☐ KO |
| 1.8 | `Logging.LogLevel.Default` | `Information` | ☐ OK / ☐ KO |
| 1.9 | `Logging.LogLevel.Microsoft` | `Warning` | ☐ OK / ☐ KO |
| 1.10 | `Serilog.WriteTo[].Name` | `File` | ☐ OK / ☐ KO |

### 1.2 Validité JSON

| # | Action | Résultat |
|---|--------|----------|
| 1.2.1 | Parser le fichier JSON | ☐ Valide / ☐ Invalide |
| 1.2.2 | Charger dans `ConfigurationManager` | �at Succès / ☐ Échec |

---

## 2. Logging

| # | Vérification | Résultat |
|---|-------------|----------|
| 2.1 | Fichier log créé dans `%ProgramData%\BMPharma\logs\` | ☐ OK / ☐ KO |
| 2.2 | Format structuré (timestamp, level, correlationId) | ☐ OK / ☐ KO |
| 2.3 | Rotation configurée (taille max 50MB) | ☐ OK / ☐ KO |
| 2.4 | Niveau `Information` en production | ☐ OK / ☐ KO |
| 2.5 | Niveau `Warning` pour Microsoft.EntityFrameworkCore | ☐ OK / ☐ KO |
| 2.6 | CorrelationId présent dans chaque entrée | ☐ OK / ☐ KO |

---

## 3. Monitoring

| # | Vérification | Résultat |
|---|-------------|----------|
| 3.1 | Dashboard affiche les métriques | ☐ OK / ☐ KO |
| 3.2 | Auto-refresh actif (60s) | ☐ OK / ☐ KO |
| 3.3 | Indicateur connexion PostgreSQL | ☐ OK / ☐ KO |
| 3.4 | Indicateur connexion CHIFA | ☐ OK / ☐ KO |
| 3.5 | Compteur factures du jour | ☐ OK / ☐ KO |
| 3.6 | Badge d'erreur visible si seuil dépassé | ☐ OK / ☐ KO |

---

## 4. Backup

| # | Vérification | Résultat |
|---|-------------|----------|
| 4.1 | Script `backup-pg.ps1` présent | ☐ OK / ☐ KO |
| 4.2 | Planification Windows Task Scheduler (02:00) | ☐ OK / ☐ KO |
| 4.3 | Répertoire backup `%ProgramData%\BMPharma\backups\pg\` | ☐ OK / ☐ KO |
| 4.4 | Backup PostgreSQL exécutable : `pg_dump` accessible | ☐ OK / ☐ KO |
| 4.5 | Vérification taille backup > 1KB | ☐ OK / ☐ KO |

---

## 5. Retry Policy

| # | Vérification | Résultat |
|---|-------------|----------|
| 5.1 | Retry configuré sur connexion CHIFA | ☐ OK / ☐ KO |
| 5.2 | Retry configuré sur connexion PostgreSQL | ☐ OK / ☐ KO |
| 5.3 | Nombre de tentatives : 3 | ☐ OK / ☐ KO |
| 5.4 | Délai entre tentatives : exponentiel | ☐ OK / ☐ KO |

---

## 6. Circuit Breaker

| # | Vérification | Résultat |
|---|-------------|----------|
| 6.1 | Circuit breaker actif pour `ChifaIntegration` | ☐ OK / ☐ KO |
| 6.2 | Seuil d'ouverture : 3 échecs | ☐ OK / ☐ KO |
| 6.3 | Durée d'ouverture : 5 min (300000ms) | ☐ OK / ☐ KO |
| 6.4 | Message UI affiché quand circuit breaker ouvert | ☐ OK / ☐ KO |
| 6.5 | Log émis lors du changement d'état | ☐ OK / ☐ KO |

---

## 7. Health Checks

| # | Vérification | Résultat |
|---|-------------|----------|
| 7.1 | CLI `--quick` exécutable | ☐ OK / ☐ KO |
| 7.2 | CLI `--full` exécutable | ☐ OK / ☐ KO |
| 7.3 | CLI `--json` sortie valide | ☐at OK / ☐ KO |
| 7.4 | UI Diagnostic Center accessible | ☐ OK / ☐ KO |
| 7.5 | Support Bundle générable | ☐ OK / ☐ KO |

---

## Anomalies

| # | Composant | Description | Impact | Criticité |
|---|-----------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Composant | Résultat |
|-----------|----------|
| appsettings.json | ☐ OK / ☐ KO |
| Logging | ☐ OK / ☐ KO |
| Monitoring | ☐ OK / ☐ KO |
| Backup | ☐ OK / ☐ KO |
| Retry | ☐ OK / ☐ KO |
| Circuit Breaker | ☐ OK / ☐ KO |
| Health Checks | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
