# 016-E — Stability Test

**Phase:** 016 — Production Pilot Deployment & Field Validation  
**Date:** 2026-07-29  
**Objective:** Mesurer la stabilité de BM Pharma sur une période prolongée.

---

## Environnement

| Champ | Valeur |
|-------|--------|
| **Machine** | |
| **Auteur test** | |
| **Période** | J+0 → J+7 (ReadOnly) / J+7 → J+14 |
| **Durée totale** | heures |

---

## 1. Collecte des Métriques

### 1.1 Mémoire

| Jour | Min (MB) | Max (MB) | Moy (MB) | Seuil (256 MB) |
|------|----------|----------|----------|----------------|
| J+0 | | | | ☐ OK / ☐ KO |
| J+1 | | | | ☐ OK / ☐ KO |
| J+2 | | | | ☐ OK / ☐ KO |
| J+3 | | | | ☐ OK / ☐ KO |
| J+4 | | | | ☐ OK / ☐ KO |
| J+5 | | | | ☐ OK / ☐ KO |
| J+6 | | | | ☐ OK / ☐ KO |
| J+7 | | | | ☐ OK / ☐ KO |

### 1.2 CPU

| Jour | Min (%) | Max (%) | Moy (%) |
|------|---------|---------|---------|
| J+0 | | | |
| J+1 | | | |
| J+2 | | | |
| J+3 | | | |
| J+4 | | | |
| J+5 | | | |
| J+6 | | | |
| J+7 | | | |

### 1.3 Handles

| Jour | Handles (moy) | Limite | Résultat |
|------|--------------|--------|----------|
| J+0 | | 10000 | ☐ OK / ☐ KO |
| J+1 | | 10000 | ☐ OK / ☐ KO |
| J+2 | | 10000 | ☐ OK / ☐ KO |
| J+3 | | 10000 | ☐ OK / ☐ KO |
| J+4 | | 10000 | ☐ OK / ☐ KO |
| J+5 | | 10000 | ☐ OK / ☐ KO |
| J+6 | | 10000 | ☐ OK / ☐ KO |
| J+7 | | 10000 | ☐ OK / ☐ KO |

### 1.4 Disponibilité

| Jour | Uptime (h) | Disponibilité (%) | Résultat |
|------|-----------|-------------------|----------|
| J+0 | | | ☐ OK / ☐ KO |
| J+1 | | | ☐ OK / ☐ KO |
| J+2 | | | ☐ OK / ☐ KO |
| J+3 | | | ☐ OK / ☐ KO |
| J+4 | | | ☐ OK / ☐ KO |
| J+5 | | | ☐ OK / ☐ KO |
| J+6 | | | ☐ OK / ☐ KO |
| J+7 | | | ☐ OK / ☐ KO |

---

## 2. Exceptions et Erreurs

| Jour | Total Exceptions | Bloquantes | Non-bloquantes |
|------|-----------------|-----------|----------------|
| J+0 | | | |
| J+1 | | | |
| J+2 | | | |
| J+3 | | | |
| J+4 | | | |
| J+5 | | | |
| J+6 | | | |
| J+7 | | | |

### Analyse des exceptions

| Exception | Fréquence | Cause | Résolution |
|-----------|-----------|-------|------------|
| | | | |

---

## 3. Reconnexions PostgreSQL

| Jour | Reconnexions | Cause | Durée indispo |
|------|-------------|-------|---------------|
| J+0 | | | |
| J+1 | | | |
| J+2 | | | |
| J+3 | | | |
| J+4 | | | |
| J+5 | | | |
| J+6 | | | |
| J+7 | | | |

---

## 4. Retry

| Jour | Tentatives retry | Succès après retry | Échec permanent |
|------|-----------------|-------------------|-----------------|
| J+0 | | | |
| J+1 | | | |
| J+2 | | | |
| J+3 | | | |
| J+4 | | | |
| J+5 | | | |
| J+6 | | | |
| J+7 | | | |

---

## 5. Synthèse

| Métrique | Moyenne | Seuil | Résultat |
|----------|---------|-------|----------|
| Mémoire (MB) | | < 256 | ☐ OK / ☐ KO |
| CPU (%) | | < 50% | ☐ OK / ☐ KO |
| Handles | | < 10000 | ☐ OK / ☐ KO |
| Disponibilité (%) | | > 99% | ☐ OK / ☐ KO |
| Exceptions bloquantes / jour | | 0 | ☐ OK / ☐ KO |
| Reconnexions / jour | | < 3 | ☐ OK / ☐ KO |
| **Global** | | | **☐ OK / ☐ KO** |

---

## Anomalies

| # | Jour | Description | Impact | Criticité |
|---|------|-------------|--------|-----------|
| | | | | |

---

## Conclusion

| Critère | Résultat |
|---------|----------|
| Stabilité mémoire | ☐ OK / ☐ KO |
| Stabilité CPU | ☐ OK / ☐ KO |
| Disponibilité | ☐ OK / ☐ KO |
| Gestion des exceptions | ☐ OK / ☐ KO |
| Reconnexions PostgreSQL | ☐ OK / ☐ KO |
| **Global** | **☐ OK / ☐ KO** |
