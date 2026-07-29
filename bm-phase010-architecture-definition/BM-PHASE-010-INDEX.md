# BM-PHASE-010 — CENTRAL DOCUMENT INDEX

**Version:** 1.0
**Date:** 2026-07-29
**Status:** ACTIVE

---

## Purpose

This index is the master navigation document for BM Pharma CHIFA integration architecture. It lists all architecture documents, their status, and their relationships.

---

## Phase 010 Deliverables (12 documents)

All in `bm-phase010-architecture-definition/`:

| # | Document | Status | Type | Source Phases |
|---|----------|--------|------|---------------|
| 1 | BM-PHASE-010-ARCHITECTURE-OVERVIEW.md | ✅ ACTIVE | Architecture | 001–009-C |
| 2 | BM-PHASE-010-DATABASE-CONTRACT.md | ✅ ACTIVE | Contract | 004.9–004.12 |
| 3 | BM-PHASE-010-INVOICE-CONTRACT.md | ✅ ACTIVE | Contract | 007 |
| 4 | BM-PHASE-010-BORDEREAU-CONTRACT.md | ✅ ACTIVE | Contract | 008, 009, 009-C |
| 5 | BM-PHASE-010-TRANSACTION-STRATEGY.md | ✅ ACTIVE | Strategy | 006, 007 |
| 6 | BM-PHASE-010-SECURITY-ARCHITECTURE.md | ✅ ACTIVE | Security | 004.8, 009-C |
| 7 | BM-PHASE-010-STATE-MACHINE.md | ✅ ACTIVE | State Machine | 003, 008 |
| 8 | BM-PHASE-010-TEST-PLAN.md | ✅ ACTIVE | Test | 004.7, 004.12 |
| 9 | BM-PHASE-010-SERVICE-ARCHITECTURE-DI.md | ✅ ACTIVE | Architecture | 001–003, 006 |
| 10 | BM-PHASE-010-ERROR-HANDLING-RESILIENCE.md | ✅ ACTIVE | Operations | 004.7, 005, 007 |
| 11 | BM-PHASE-010-OPERATIONS-GUIDE.md | ✅ ACTIVE | Operations | 001–009-C |
| 12 | BM-PHASE-010-RISK-REGISTER-DECISION-LOG.md | ✅ ACTIVE | Risk | 001–009-C |

---

## Historical Contracts (Archived)

Archived in `bm-phase010-architecture-definition/archive/pre-phase010/`:

| # | Original File | Archived | Status |
|---|---------------|----------|--------|
| 1 | BM_PHARMA_CHIFA_INTEGRATION_ARCHITECTURE.md | ✅ Archived | REPLACED BY v2.0 (Doc 1) |
| 2 | BM_PHARMA_CHIFA_DATABASE_CONTRACT.md | ✅ Archived | REPLACED BY v2.0 (Doc 2) |
| 3 | BM_PHARMA_CHIFA_BORDEREAU_CONTRACT.md | ✅ Archived | REPLACED BY v2.0 (Doc 4) |
| 4 | BM_PHARMA_CHIFA_INVOICE_CONTRACT.md | ✅ Archived | REPLACED BY v2.0 (Doc 3) |
| 5 | BM_PHARMA_CHIFA_SECURITY.md | ✅ Archived | REPLACED BY v2.0 (Doc 6) |
| 6 | BM_PHARMA_CHIFA_STATE_MACHINE.md | ✅ Archived | REPLACED BY v2.0 (Doc 7) |
| 7 | BM_PHARMA_CHIFA_TEST_PLAN.md | ✅ Archived | REPLACED BY v2.0 (Doc 8) |
| 8 | BM_PHARMA_CHIFA_TRANSACTION_STRATEGY.md | ✅ Archived | REPLACED BY v2.0 (Doc 5) |

---

## Revised Contracts (Root — v2.0 headers added)

The 8 original contract files in the root directory have been updated with v2.0 headers (version, date, status, source of truth, history). Their content remains for backward compatibility; the full v2.0 content is in the Phase 010 documents.

| # | File in Root | Now Links To |
|---|-------------|-------------|
| 1 | BM_PHARMA_CHIFA_INTEGRATION_ARCHITECTURE.md | Doc 1 (ARCHITECTURE-OVERVIEW) |
| 2 | BM_PHARMA_CHIFA_DATABASE_CONTRACT.md | Doc 2 (DATABASE-CONTRACT) |
| 3 | BM_PHARMA_CHIFA_BORDEREAU_CONTRACT.md | Doc 4 (BORDEREAU-CONTRACT) |
| 4 | BM_PHARMA_CHIFA_INVOICE_CONTRACT.md | Doc 3 (INVOICE-CONTRACT) |
| 5 | BM_PHARMA_CHIFA_SECURITY.md | Doc 6 (SECURITY-ARCHITECTURE) |
| 6 | BM_PHARMA_CHIFA_STATE_MACHINE.md | Doc 7 (STATE-MACHINE) |
| 7 | BM_PHARMA_CHIFA_TEST_PLAN.md | Doc 8 (TEST-PLAN) |
| 8 | BM_PHARMA_CHIFA_TRANSACTION_STRATEGY.md | Doc 5 (TRANSACTION-STRATEGY) |

---

## Source Phase Documents

### Phases 001–003 (Architecture Foundations)

| Document | Location | Key Content |
|----------|----------|-------------|
| BM-PHASE-001-REPORT.md | Root | Solution bootstrap, clean architecture, 19 projects |
| BM-PHASE-002-REPORT.md | Root | CHIFA integration core, 4 interfaces, 4 stubs, 28 tests |
| BM-PHASE-002-AUDIT.md | Root | Solution audit report |
| BM-PHASE-003-REPORT.md | Root | Workflow integration, state machine, 158 tests |
| BM-PHASE-003-INTEGRATION-REPORT.md | Root | Integration architecture |
| BM-PHASE-003-WORKFLOW-STATE-MACHINE.md | Root | State machine definitions |
| BM-PHASE-003-CHIFA-MAPPING.md | Root | CHIFA mapping details |
| BM-PHASE-003-CODE-AUDIT.md | Root | Code audit |
| BM-PHASE-003-ERROR-MATRIX.md | Root | Error handling matrix |
| BM-PHASE-003-MANUAL-TEST-PLAN.md | Root | Manual test plan |

### Phases 004.3–004.12 (Schema Discovery & Validation)

| Document | Location | Key Content |
|----------|----------|-------------|
| BM-PHASE-004.3-FINAL-REPORT.md | Root | First schema discovery (Docker PG 16) |
| BM-PHASE-004.4-FINAL-REPORT.md | Root | WPF dashboard completion |
| BM-PHASE-004.5-FINAL-REPORT.md | Root | Invoice preparation workflow |
| BM-PHASE-004.6-FINAL-REPORT.md | Root | Bordereau status & monitoring |
| BM-PHASE-004.7-FINAL-REPORT.md | Root | Production readiness (429 tests) |
| BM-PHASE-004.8-FINAL-REPORT.md | Root | First real PG schema discovery (BLOCKED) |
| BM-PHASE-004.8-REAL-SCHEMA.md | Root | Real schema (when available) |
| BM-PHASE-004.8-READONLY-EVIDENCE.md | Root | ReadOnly mode evidence |
| BM-PHASE-004.8-SECURITY-REPORT.md | Root | Security report |
| BM-PHASE-004.8-COMPATIBILITY-MATRIX.md | Root | EF Core compatibility |
| BM-PHASE-004.8-EFCORE-COMPATIBILITY.md | Root | EF Core compatibility details |
| BM-PHASE-004.9-ARCHITECTURE-RECOMMENDATION.md | Root | Architecture recommendation (Option A) |
| BM-PHASE-004.9-FINAL-REPORT.md | Root | Real environment discovery (48 tables) |
| BM-PHASE-004.9-REAL-DATABASE.md | Root | Real database analysis |
| BM-PHASE-004.9-REAL-ENVIRONMENT.md | Root | Real environment details |
| BM-PHASE-004.9-REAL-SCHEMA.md | Root | Real schema |
| BM-PHASE-004.9-CORE-TABLES.md | Root | Core tables analysis |
| BM-PHASE-004.9-RELATIONSHIPS.md | Root | Table relationships |
| BM-PHASE-004.9-SPEC-RECONCILIATION.md | Root | Spec reconciliation |
| BM-PHASE-004.9-CONNECTION-ANALYSIS.md | Root | Connection analysis |
| BM-PHASE-004.9-NEW-TABLES-ANALYSIS.md | Root | New tables analysis |
| BM-PHASE-004.9-BM-EFCORE-VALIDATION.md | Root | EF Core validation |
| BM-PHASE-004.9-SECURITY-REPORT.md | Root | Security report |
| BM-PHASE-004.10-FINAL-REPORT.md | Root | Entity correction (0 phantom properties) |
| BM-PHASE-004.10-INTEGRATION-ARCHITECTURE.md | Root | Integration architecture |
| BM-PHASE-004.10-EFCORE-MAPPING.md | Root | EF Core mapping |
| BM-PHASE-004.10-CORE-TABLES.md | Root | Core tables |
| BM-PHASE-004.10-DOCKER-VS-REAL.md | Root | Docker vs real comparison |
| BM-PHASE-004.10-MEDICAMENT-ANALYSIS.md | Root | Medicament analysis |
| BM-PHASE-004.10-LN-ANALYSIS.md | Root | LN analysis |
| BM-PHASE-004.11-FINAL-REPORT.md | Root | PG TCP crash diagnosis (permission, not crash) |
| BM-PHASE-004.12-FINAL-REPORT.md | Root | Final real PG validation (509 tests) |
| BM-PHASE-004.12-SCHEMA-DISCOVERY.md | Root | Full schema inventory |
| BM-PHASE-004.12-CRITICAL-TABLES.md | Root | Critical tables deep-dive |
| BM-PHASE-004.12-READONLY-EVIDENCE.md | Root | ReadOnly evidence |
| BM-PHASE-004.12-DOCKER-VS-REAL.md | Root | Docker vs real comparison |
| BM-PHASE-004.12-EFCORE-VALIDATION.md | Root | EF Core validation |
| BM-PHASE-004.12-REAL-CONNECTION.md | Root | Connection validation |

### Phases 005–007 (Write Validation)

| Document | Location | Key Content |
|----------|----------|-------------|
| BM-PHASE-005-FINAL-REPORT.md | Root | TST001 write + rollback (12/12 checks) |
| BM-PHASE-005-PREWRITE-SNAPSHOT.md | Root | Pre-write baseline |
| BM-PHASE-006-FINAL-REPORT.md | Root | EF Core integration, counters, transactions |
| BM-PHASE-006-EFCORE-INTEGRATION.md | Root | EF Core integration details |
| BM-PHASE-006-D-COUNTER-ANALYSIS.md | Root | Atomic counters (UPDATE...RETURNING) |
| BM-PHASE-006-TRANSACTION-STRATEGY.md | Root | Transaction strategy |
| BM-PHASE-006-MODE-SECURITY.md | Root | Mode security |
| BM-PHASE-006-TEST-REPORT.md | Root | Test report (509 passing) |
| BM-PHASE-006-REAL-INTEGRATION-TEST-PLAN.md | Root | Real integration test plan |
| BM-PHASE-006-A-CODE-AUDIT.md | Root | Code audit |
| BM-PHASE-007-G-REAL-WRITE-REPORT.md | Root | TST002 EF Core write (3 bugs found + fixed) |
| BM-PHASE-007-I-ROLLBACK-REPORT.md | Root | TST002 rollback (10/10 checks) |
| BM-PHASE-007-A-PRETEST-AUDIT.md | Root | Pre-test audit |
| BM-PHASE-007-B-PREWRITE-SNAPSHOT.md | Root | Pre-write snapshot |
| BM-PHASE-007-C-TEST-DATA.md | Root | Test data |
| BM-PHASE-007-D-PROTOCOL-VALIDATION.md | Root | Protocol validation |
| BM-PHASE-007-E-SIMULATION-TEST.md | Root | Simulation test |
| BM-PHASE-008-B-ROLLBACK-REPORT.md | Root | TST003 rollback |

### Phases 008–009-C (Lifecycle, SQL, Memory Scan)

| Document | Location | Key Content |
|----------|----------|-------------|
| bm-phase008-lifecycle-audit/ | Subdirectory | Full lifecycle audit |
| BM-PHASE-008-A-WORKFLOW-MATRIX.md | Subdir | Workflow matrix (green/orange/red) |
| BM-PHASE-008-A-SIGNING-FLOW.md | Subdir | PKCS#7 signing flow |
| BM-PHASE-008-A-CLOSING-FLOW.md | Subdir | Bordereau closure flow |
| BM-PHASE-008-A-CNAS-TRANSMISSION.md | Subdir | CNAS FTP transmission |
| BM-PHASE-008-A-VISIBILITY-ANALYSIS.md | Subdir | Visibility analysis |
| BM-PHASE-008-A-EVIDENCE-MATRIX.md | Subdir | Evidence matrix |
| BM-PHASE-008-A-EXECUTIVE-REPORT.md | Subdir | Executive report |
| BM-PHASE-008-A-BORDEREAU-LIFECYCLE.md | Subdir | Bordereau lifecycle |
| BM-PHASE-008-A-INVOICE-LIFECYCLE.md | Subdir | Invoice lifecycle |
| bm-phase009-bordereau-reverse-engineering/ | Subdirectory | Bordereau reverse engineering |
| BM-PHASE-009-ARCHITECTURE-RECOMMENDATION.md | Subdir | Final architecture recommendation (Option D) |
| BM-PHASE-009-SQL-ANALYSIS.md | Subdir | SQL analysis |
| BM-PHASE-009-DETAIL-BORD-ANALYSIS.md | Subdir | detail_bord analysis |
| BM-PHASE-009-EVIDENCE-MATRIX.md | Subdir | Evidence matrix |
| BM-PHASE-009-FINAL-REPORT.md | Subdir | Final report |
| BM-PHASE-009-SQL-CAPTURE-REPORT.md | Subdir | SQL capture report |
| BM-PHASE-009-TST003-ROOT-CAUSE.md | Subdir | TST003 root cause (6 hypotheses) |
| BM-PHASE-009-VISIBILITY-RULES.md | Subdir | Visibility rules |
| BM-PHASE-009-VISUALISER-BORDEREAU-CALLGRAPH.md | Subdir | Call graph |
| BM-PHASE-009-CHIFA-WORKFLOW.md | Subdir | CHIFA workflow |
| bm-phase009-c-npgsql-memory-scan/ | Subdirectory | Memory scan (Windbg) |
| BM-PHASE-009-C-MEMORY-SCAN-RESULTS.md | Subdir | Memory scan results |
| BM-PHASE-009-C-SQL-FINDINGS.md | Subdir | 4 SQL families confirmed |
| BM-PHASE-009-C-DETAIL-BORD-ANALYSIS.md | Subdir | detail_bord = DataTable confirmed |
| BM-PHASE-009-C-CONFIDENCE-MATRIX.md | Subdir | Confidence matrix |
| BM-PHASE-009-C-TST003-RECONCILIATION.md | Subdir | TST003 reconciliation |
| BM-PHASE-009-C-SECURITY-EVIDENCE.md | Subdir | Credential leakage evidence |
| BM-PHASE-009-C-PROCESS-IDENTIFICATION.md | Subdir | Process identification |
| BM-PHASE-009-C-EXECUTIVE-REPORT.md | Subdir | Executive report |

---

## Certitude Matrix (Summary)

| Element | Status |
|---------|--------|
| PostgreSQL real (9.3.4 32-bit) | CONFIRMED |
| EF Core mapping (6 entities) | CONFIRMED |
| Write facture + detail_fact via EF Core | CONFIRMED |
| Rollback to exact baseline | CONFIRMED |
| Atomic counters (UPDATE...RETURNING) | CONFIRMED |
| ReadOnly 3-layer protection | CONFIRMED |
| Invoice visible in CHIFA UI | CONFIRMED |
| Bordereau visible in CHIFA UI | PARTIAL |
| detail_bord = .NET DataTable (not PG table) | CONFIRMED |
| detail_bord filling algorithm | UNKNOWN |
| 4 SQL query families | CONFIRMED |
| Signing from BM Pharma | NOT SUPPORTED |
| Cloture from BM Pharma | NOT SUPPORTED |
| CNAS transmission from BM Pharma | NOT SUPPORTED |
| Credentials in CHIFA process memory | CONFIRMED |
| SQL in CHIFA process memory | CONFIRMED |
| 509 tests passing | CONFIRMED |
| TST001/TST002/TST003 all rolled back | CONFIRMED |

---

## Architecture Decision (Final)

**Recommended Architecture: OPTION D (Hybrid)**

```
BM Pharma writes:    facture + detail_fact only
CHIFA handles:       bordereau, signature, cloture, CNAS transmission
BM Pharma monitors:  SELECT status columns
```

---

## Known Limitations

1. **Bordereau visibility** — BM Pharma-created bordereaux may be invisible in Visualiser Bordereau (TST003)
2. **detail_bord algorithm** — Not fully determined; runtime SQL parameters partially unknown
3. **Signing/cloture/transmission** — Require CHIFA-OFFICINE; cannot be automated by BM Pharma
4. **RBAC** — Not implemented; all operations use "system" user
5. **AdminPassword** — Currently hardcoded in appsettings.json
6. **PostgreSQL 9.3.4** — EOL, 32-bit, crash-prone legacy

---

## Next Phase Recommendation

**BM-PHASE-011**: Execute the first write operation against the real CHIFA_OFFICINE database using the validated Option D protocol.

**Prerequisites:**
- [ ] Explicit approval from user
- [ ] Backup of CHIFA_OFFICINE database
- [ ] Mode set to Test (not Production)
- [ ] Baseline documented
- [ ] Rollback plan ready

---

## Total Document Count

| Category | Count |
|----------|-------|
| Phase 010 deliverables | 12 |
| Phase 010 index + final report | 2 |
| Archived contracts (pre-Phase 010) | 8 |
| Revised contracts (root, with v2.0 headers) | 8 |
| Source phase documents (root) | ~62 |
| Source phase documents (subdirectories) | ~29 |
| **Total repository** | **~121 .md files** |
