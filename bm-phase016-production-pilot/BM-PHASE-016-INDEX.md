# BM Pharma — Phase 016 — Production Pilot Deployment & Field Validation — Index

**Date:** 2026-07-29  
**Version:** 2.0.0  

---

## Document Index

| Ref | Document | Description | Status |
|-----|----------|-------------|--------|
| **016-A** | [Clean Environment Deployment](BM-PHASE-016-A-DEPLOYMENT.md) | Installation, dépendances, désinstallation | ☐ |
| **016-B** | [Diagnostic Center Validation](BM-PHASE-016-B-DIAGNOSTIC.md) | Exécution et validation des 18 tests | ☐ |
| **016-C** | [Production Configuration](BM-PHASE-016-C-CONFIGURATION.md) | Validation appsettings, logs, monitoring, backup, retry, circuit breaker, health checks | ☐ |
| **016-D** | [End-to-End Pilot Workflow](BM-PHASE-016-D-END-TO-END.md) | Workflow complet Vente → CNAS | ☐ |
| **016-E** | [Stability Test](BM-PHASE-016-E-STABILITY.md) | Mémoire, CPU, handles, disponibilité multi-jours | ☐ |
| **016-F** | [Backup & Restore Validation](BM-PHASE-016-F-BACKUP-RESTORE.md) | Sauvegarde, restauration, rollback, reprise | ☐ |
| **016-G** | [Failure Injection](BM-PHASE-016-G-FAILURE-INJECTION.md) | PostgreSQL, CHIFA, réseau, SAM, disque | ☐ |
| **016-H** | [Monitoring Validation](BM-PHASE-016-H-MONITORING.md) | Métriques, dashboards, logs, correlation IDs, health checks, circuit breaker | ☐ |
| **016-I** | [Pharmacist Pilot Feedback](BM-PHASE-016-I-PILOT-FEEDBACK.md) | Retours pharmacien : ergonomie, rapidité, satisfaction | ☐ |
| **016-J** | [Production Readiness Review](BM-PHASE-016-J-PRODUCTION-REVIEW.md) | Comparaison critères Phase 015, scoring, risques | ☐ |
| **016-K** | [Go / No-Go Committee](BM-PHASE-016-K-GO-NOGO.md) | Décision officielle motivée | ☐ |
| **016-FINAL** | [Final Report](BM-PHASE-016-FINAL-REPORT.md) | Rapport de synthèse Phase 016 | ☐ |

---

## Workflow d'Exécution

```
016-A ──▶ 016-B ──▶ 016-C ──▶ 016-D ──▶ 016-E ──▶ 016-F
                                  │                    │
                                  ▼                    ▼
                              016-I ◀─── 016-H ──── 016-G
                                  │
                                  ▼
                              016-J ──▶ 016-K ──▶ STOP
```

---

## Dépendances

| Document | Dépend de | Fournit |
|----------|-----------|---------|
| 016-A | — | Environnement propre |
| 016-B | 016-A | Validation diagnostic |
| 016-C | 016-A | Configuration validée |
| 016-D | 016-B, 016-C | Workflow validé |
| 016-E | 016-D | Stabilité mesurée |
| 016-F | 016-E | Backup/Restore validé |
| 016-G | 016-E | Résilience validée |
| 016-H | 016-E, 016-G | Monitoring validé |
| 016-I | 016-D, 016-H | Retour pharmacien |
| 016-J | 016-E, 016-F, 016-G, 016-H, 016-I | Score final |
| 016-K | 016-J | Décision |
| 016-FINAL | Tous | Rapport |

---

## Règles

| Règle | Description |
|-------|-------------|
| **R1** | Aucun changement d'architecture |
| **R2** | Aucune nouvelle fonctionnalité métier |
| **R3** | Toutes les validations sur environnement représentatif |
| **R4** | Toutes les anomalies documentées (impact + criticité) |
| **R5** | Décision motivée par les résultats observés |
| **R6** | STOP après 016-K — attendre approbation avant Phase 017 |
