# 014 — User Acceptance Validation (UAV) — Index

**Date:** 2026-07-29  
**Status:** ✅ Complete  
**Décision:** GO PILOT ONLY  

---

## Documents

| # | Document | Description | Pages |
|---|----------|-------------|-------|
| **014-A** | `BM-PHASE-014-A-FUNCTIONAL-WORKFLOW.md` | Validation pas à pas du workflow Vente → CNAS (9 étapes, 50+ points de validation) | 5 |
| **014-B** | `BM-PHASE-014-B-UAT-CHECKLIST.md` | Checklist pharmacien : 84 cases à cocher (9 sections) | 6 |
| **014-C** | `BM-PHASE-014-C-ERROR-SCENARIOS.md` | 10 scénarios d'erreur (symptômes, détection, message, récupération, matrice de gravité) | 6 |
| **014-D** | `BM-PHASE-014-D-UI-VALIDATION.md` | Validation des 4 vues (70+ composants vérifiés) | 5 |
| **014-E** | `BM-PHASE-014-E-PERFORMANCE-VALIDATION.md` | 7 mesures de performance (dashboard: 490ms, écriture: 690ms, etc.) | 4 |
| **014-F** | `BM-PHASE-014-F-OPERATOR-MANUAL.md` | Guide utilisateur : installation, configuration, workflow, FAQ, support | 8 |
| **014-G** | `BM-PHASE-014-G-ADMINISTRATOR-GUIDE.md` | Guide admin : sauvegarde, restauration, logs, monitoring, diagnostic | 7 |
| **014-H** | `BM-PHASE-014-H-PILOT-DEPLOYMENT.md` | Checklist déploiement : 87 cases (8 sections) | 6 |
| **014-I** | `BM-PHASE-014-I-FINAL-GO-NOGO.md` | Décision GO PILOT ONLY avec 6 conditions | 5 |
| **014-FINAL** | `BM-PHASE-014-FINAL-REPORT.md` | Rapport de synthèse de la phase | 3 |

---

## Références Traçables

| Document Source | Lié à |
|----------------|-------|
| `BM-PHASE-012-I-FINAL-ASSESSMENT.md` | Décision GO WITH LIMITATIONS → GO PILOT ONLY |
| `BM-PHASE-013-UI-INTEGRATION.md` | Monitoring dashboard, auto-refresh, exception mapper |
| `BM-PHASE-007-G-REAL-WRITE-REPORT.md` | Preuve écriture réelle EF Core (877ms) |
| `BM-PHASE-006-FINAL-REPORT.md` | 509/509 tests, architecture Read/Write |
| `BM_PHARMA_CHIFA_TEST_PLAN.md` | Plan de test général |
| `BM-PHASE-012-C-EXCEPTION-AUDIT.md` | 7 types d'exception avec propagation |
| `BM-PHASE-012-E-SECURITY-REVIEW.md` | Risques sécurité documentés |

---

## Acronymes

| Acronyme | Signification |
|----------|--------------|
| UAV | User Acceptance Validation |
| UAT | User Acceptance Test |
| CB | Circuit Breaker |
| CNAS | Caisse Nationale des Assurances Sociales |
| SAM | Security Access Module (token PKCS#11) |
| PG | PostgreSQL |
| CHIFA | Chaîne d'Informatisation de la Filière d'Assurance |
| EF Core | Entity Framework Core |
| DI | Dependency Injection |
| MVP | Minimum Viable Product |
