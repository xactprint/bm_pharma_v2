# 016-H — Monitoring Validation

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Valider les métriques, dashboards, logs, correlation IDs, health checks et circuit breaker.

---

## Environnement

| Champ | Valeur |
|-------|--------|
| **Auteur test** | |
| **Date** | |

---

## 1. Métriques Dashboard

| # | Métrique | Affichée | Valeur | Correcte |
|---|----------|----------|--------|----------|
| 1.1 | Connexion PostgreSQL | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |
| 1.2 | Connexion CHIFA | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |
| 1.3 | État synchronisation | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |
| 1.4 | Factures du jour | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |
| 1.5 | Stock OK / Alert | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |
| 1.6 | Erreurs dernière heure | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |
| 1.7 | Build version | ☐ Oui / ☐ Non | | ☐ OK / ☐ KO |

---

## 2. Auto-Refresh

| # | Vérification | Résultat |
|---|-------------|----------|
| 2.1 | Les métriques se rafraîchissent toutes les 60s | ☐ OK / ☐ KO |
| 2.2 | Le timer s'arrête quand la fenêtre est cachée | ☐ OK / ☐ KO |
| 2.3 | Le timer reprend quand la fenêtre est visible | ☐ OK / ☐ KO |
| 2.4 | Pas de freeze UI pendant le refresh | ☐ OK / ☐ KO |
| 2.5 | Les erreurs de refresh sont catchées (pas de crash) | ☐ OK / ☐ KO |

---

## 3. Logs

| # | Vérification | Résultat |
|---|-------------|----------|
| 3.1 | Fichier log créé dans `%ProgramData%\BMPharma\logs\` | ☐ OK / ☐ KO |
| 3.2 | Format : `2026-07-29T08:30:15 [INF] (CID: ...) message` | ☐ OK / ☐ KO |
| 3.3 | CorrelationId présent dans chaque ligne | ☐ OK / ☐ KO |
| 3.4 | Niveaux visibles : INF, DBG, WRN, ERR, FTL | ☐ OK / ☐ KO |
| 3.5 | Rotation log fonctionnelle (taille max 50MB) | ☐ OK / ☐ KO |
| 3.6 | Log de démarrage de l'application | ☐ OK / ☐ KO |

---

## 4. Correlation IDs

| # | Vérification | Résultat |
|---|-------------|----------|
| 4.1 | CorrelationId généré au démarrage | ☐ OK / ☐ KO |
| 4.2 | Même CorrelationId dans toute une session utilisateur | ☐ OK / ☐ KO |
| 4.3 | CorrelationId présent dans les logs de synchronisation | ☐ OK / ☐ KO |
| 4.4 | CorrelationId présent dans Support Bundle | ☐ OK / ☐ KO |

---

## 5. Health Checks

### 5.1 CLI --quick

| # | Vérification | Résultat |
|---|-------------|----------|
| 5.1.1 | `BMPharma.Diagnostic.exe --quick` s'exécute | ☐ OK / ☐ KO |
| 5.1.2 | Durée < 5s | ☐ OK / ☐ KO |
| 5.1.3 | Tous les tests T01-T06 passent | ☐ OK / ☐ KO |

### 5.2 CLI --full

| # | Vérification | Résultat |
|---|-------------|----------|
| 5.2.1 | `BMPharma.Diagnostic.exe --full` s'exécute | ☐ OK / ☐ KO |
| 5.2.2 | Durée < 30s | ☐ OK / ☐ KO |
| 5.2.3 | Tous les tests T01-T18 passent | ☐ OK / ☐ KO |

### 5.3 CLI --json

| # | Vérification | Résultat |
|---|-------------|----------|
| 5.3.1 | `BMPharma.Diagnostic.exe --full --json` | ☐ OK / ☐ KO |
| 5.3.2 | Sortie JSON valide | ☐ OK / ☐ KO |
| 5.3.3 | Structure correcte : timestamp, results, summary | ☐ OK / ☐ KO |

### 5.4 UI Diagnostic Center

| # | Vérification | Résultat |
|---|-------------|----------|
| 5.4.1 | Accès depuis la sidebar | ☐ OK / ☐ KO |
| 5.4.2 | Affichage des 18 tests | ☐ OK / ☐ KO |
| 5.4.3 | Bouton "Exécuter" fonctionne | ☐ OK / ☐ KO |
| 5.4.4 | Résultats vert/rouge par test | ☐ OK / ☐ KO |

---

## 6. Circuit Breaker

| # | Vérification | Résultat |
|---|-------------|----------|
| 6.1 | Circuit breaker pour `ChifaIntegration` configuré | ☐ OK / ☐ KO |
| 6.2 | Seuil d'ouverture : 3 échecs consécutifs | ☐ OK / ☐ KO |
| 6.3 | Durée d'ouverture : 5 minutes | ☐ OK / ☐ KO |
| 6.4 | Log émis à l'ouverture : "Circuit breaker state: Open" | ☐ OK / ☐ KO |
| 6.5 | Log émis à la fermeture : "Circuit breaker state: Closed" | ☐at OK / ☐ KO |
| 6.6 | Message UI quand circuit breaker est ouvert | ☐ OK / ☐ KO |

---

## 7. Support Bundle

| # | Vérification | Résultat |
|---|-------------|----------|
| 7.1 | Génération depuis CLI : `--support-bundle` | ☐ OK / ☐ KO |
| 7.2 | Génération depuis UI Diagnostic Center | ☐ OK / ☐ KO |
| 7.3 | Fichier ZIP créé | ☐ OK / ☐ KO |
| 7.4 | Structure : manifest, config, versions, logs, pg | ☐ OK / ☐ KO |
| 7.5 | Aucune donnée patient dans le bundle | ☐ OK / ☐ KO |

---

## Anomalies

| # | Composant | Description | Impact | Criticité |
|---|-----------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Composant | Résultat |
|-----------|----------|
| Métriques Dashboard | ☐ OK / ☐ KO |
| Auto-refresh | ☐ OK / ☐ KO |
| Logs | ☐ OK / ☐ KO |
| Correlation IDs | ☐ OK / ☐ KO |
| Health Checks CLI | ☐ OK / ☐ KO |
| Health Checks UI | ☐ OK / ☐ KO |
| Circuit Breaker | ☐ OK / ☐ KO |
| Support Bundle | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
