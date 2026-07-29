# 016-B — Diagnostic Center Validation

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Exécuter le Diagnostic Center et documenter tous les contrôles.

---

## Exécution

| Champ | Valeur |
|-------|--------|
| **Mode** | `--full` (18 tests) |
| **Auteur test** | |
| **Date** | |
| **Environnement** | |

---

## Résultats par Test

| Test | Nom | Attendu | Résultat | Temps | Commentaire |
|------|-----|---------|----------|-------|-------------|
| T01 | Windows Compatible | Windows 10+ 64-bit | ☐ OK / ☐ KO | ms | |
| T02 | .NET Runtime | ≥ 8.0 x64 | ☐ OK / ☐ KO | ms | |
| T03 | PostgreSQL Connection | Connecté | ☐ OK / ☐ KO | ms | |
| T04 | CHIFA Connection | Connecté | ☐ OK / ☐ KO | ms | |
| T05 | CHIFA Schema | Schéma correct | ☐ OK / ☐ KO | ms | |
| T06 | Database Permissions | `pharm` a tous les droits | ☐ OK / ☐ KO | ms | |
| T07 | CHIFA Features | Fonctionnalités CHIFA dispo | ☐ OK / ☐ KO | ms | |
| T08 | Synchronization Health | Sync OK | ☐ OK / ☐ KO | ms | |
| T09 | SSL / Certificates | Pas de certificat expiré | ☐ OK / ☐ KO | ms | |
| T10 | SAM Configuration | SAM accessible | ☐ OK / ☐ KO | ms | |
| T11 | DataDirectories | Dossiers accessibles | ☐ OK / ☐ KO | ms | |
| T12 | Logging | Logs accessibles et récents | ☐ OK / ☐ KO | ms | |
| T13 | Backup Status | Backup < 24h | ☐ OK / ☐ KO | ms | |
| T14 | Disk Space | Disque libre > 500MB | ☐ OK / ☐ KO | ms | |
| T15 | Memory Usage | < 256 MB | ☐ OK / ☐ KO | ms | |
| T16 | Network | Connexion stable | ☐ OK / ☐ KO | ms | |
| T17 | Configuration | appsettings.json valide | ☐ OK / ☐ KO | ms | |
| T18 | Update Check | Version à jour | ☐ OK / ☐ KO | ms | |

---

## Résultat Global

```
Tests réussis :    /18
Tests échoués :    /18
Temps total :      secondes
```

### Détail des Échecs (si applicable)

| Test | Raison | Action Corrective |
|------|--------|-------------------|
| | | |

---

## Mode --quick (T01-T06)

| Test | Résultat |
|------|----------|
| T01 | ☐ OK / ☐ KO |
| T02 | ☐ OK / ☐ KO |
| T03 | ☐ OK / ☐ KO |
| T04 | ☐ OK / ☐ KO |
| T05 | ☐ OK / ☐ KO |
| T06 | ☐ OK / ☐ KO |

---

## Sortie JSON

```json
{
  "timestamp": "",
  "version": "",
  "results": [
    {
      "test": "T01",
      "status": "pass/fail",
      "durationMs": 0,
      "detail": ""
    }
  ],
  "summary": {
    "total": 18,
    "passed": 0,
    "failed": 0,
    "durationMs": 0
  }
}
```

---

## Anomalies

| # | Test | Description | Impact | Criticité |
|---|------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Critère | Résultat |
|---------|----------|
| Tous les tests 18/18 passent | ☐ OK / ☐ KO |
| Mode `--quick` fonctionnel | ☐ OK / ☐ KO |
| Mode `--full` fonctionnel | ☐ OK / ☐ KO |
| Sortie JSON valide | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
