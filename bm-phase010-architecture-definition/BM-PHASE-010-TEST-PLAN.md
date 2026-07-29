# BM-PHASE-010 — TEST PLAN

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phase 004.7 (429 tests), Phase 004.12 (509 tests), Phase 007 (real write validation)
**Previous Version:** BM_PHARMA_CHIFA_TEST_PLAN.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial test plan (20 test cases) |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with 509 existing tests, real write validation, TST001/TST002/TST003 |

### Major Changes from v1.0

| Change | v1.0 | v2.0 | Source |
|--------|------|------|--------|
| Total tests | 20 (planned) | 509 (actual, passing) | Phase 004.12 |
| CHIFA tests | 20 (planned) | 493 (actual) | Phase 004.12 |
| Real write validation | None | TST001, TST002 CONFIRMED | Phase 005, 007 |
| Real PG schema validation | None | 48 tables CONFIRMED | Phase 004.12 |
| Baseline restoration | None | CONFIRMED (TST001, TST002, TST003 all rolled back) | Phase 005, 007, 008 |
| Real-world bugs found | 0 | 3 (DateTime Kind, FK ordering, column types) | Phase 007-G |

---

## Current Test Suite

Total: **509 tests, ALL PASSING, 0 failures, 0 regressions** (Phase 004.12)

| Project | Tests | Type | Status |
|---------|-------|------|--------|
| BMPharma.Domain.Tests | 6 | Unit | CONFIRMED |
| BMPharma.Application.Tests | 3 | Unit | CONFIRMED |
| BMPharma.ArchitectureTests | 7 | Architecture | CONFIRMED |
| BMPharma.CHIFA.Tests | 493 | Unit + Integration | CONFIRMED |

### CHIFA Test Breakdown (493 tests)

| Test File | Tests | Coverage |
|-----------|-------|----------|
| ChifaInvoiceValidatorTests | 11 | Invoice validation rules |
| ChifaBordereauValidatorTests | 5 | Bordereau validation |
| ChifaWriteGuardTests | 4 | Security mode guards |
| ChifaSigningServiceTests | 2 | Signing delegation |
| ChifaInvoiceServiceTests | 2 | Invoice service stubs |
| ChifaBordereauServiceTests | 4 | Bordereau service stubs |
| ChifaInvoiceMapperTests | 20 | BM→CHIFA invoice mapping |
| ChifaBordereauMapperTests | 9 | BM→CHIFA bordereau mapping |
| ChifaWorkflowStateMachineTests | 25 | State machine transitions |
| FakeChifaIntegrationProviderTests | 32 | Simulation provider |
| OneActionWorkflowServiceTests | 11 | Workflow orchestration |
| ChifaNegativeScenarioTests | 25 | Error scenarios |
| StructuredChifaAuditServiceTests | 12 | Audit logging |
| ChifaDependencyInjectionTests | 14 | DI wiring |
| ChifaDbContextTests | 20 | EF Core context |
| ChifaDashboardTests | 30 | Dashboard ViewModel |
| ChifaInvoiceWorkflowServiceTests | 40 | Invoice workflow |
| BordereauWorkflowStateMachineTests | 19 | Bordereau state machine |
| ChifaBordereauStatusServiceTests | 30 | Bordereau monitoring |
| ProductionReadinessTests | 102 | Production validation |
| ChifaRealSchemaAlignmentTests | 50 | PG schema alignment |
| ChifaReadOnlyValidationTests | 30 | ReadOnly validation (real PG) |
| RealWriteTest | 1 | Real PKCS#7 signature format validation |

---

## Real Write Tests (Executed, Rolled Back)

These were MANUAL tests executed against the real CHIFA_OFFICINE PostgreSQL, then fully rolled back.

### TST001 (Phase 005)

| Aspect | Detail |
|--------|--------|
| Method | Direct SQL via Npgsql (not EF Core) |
| Created | 1 facture + 1 detail_fact |
| Rollback | Manual DELETE FROM SQL |
| Verification | 12/12 checks, baseline restored |
| Status | CONFIRMED |

### TST002 (Phase 007)

| Aspect | Detail |
|--------|--------|
| Method | EF Core via ChifaPostgresInvoiceService |
| Created | 1 facture + 1 detail_fact |
| Duration | 877ms |
| Bugs found | 3 (DateTime Kind, FK ordering, column types) |
| Rollback | Manual DELETE FROM SQL |
| Verification | 10/10 checks, baseline restored |
| Status | CONFIRMED |

### TST003 (Phase 008)

| Aspect | Detail |
|--------|--------|
| Purpose | Visibility validation in CHIFA-OFFICINE |
| Method | EF Core write + CHIFA UI observation |
| Result | Facture visible, bordereau partially visible |
| Rollback | Completed |
| Status | CONFIRMED (visibility gap documented) |

---

## Test Categories

### Unit Tests (InMemory / No DB)

Coverage: All validation logic, mapping, state machines, guards.

| Category | Tests | Status |
|----------|-------|--------|
| Invoice validation | 11 | CONFIRMED |
| Bordereau validation | 5 | CONFIRMED |
| WriteGuard | 4 | CONFIRMED |
| Mapping | 29 | CONFIRMED |
| State machines | 44 | CONFIRMED |
| Workflow orchestration | 51 | CONFIRMED |
| Audit | 12 | CONFIRMED |
| DI wiring | 14 | CONFIRMED |
| Dashboard ViewModels | 30 | CONFIRMED |
| Security | 102 | CONFIRMED |
| **Total** | **~460** | **CONFIRMED** |

### Integration Tests (InMemory / SQLite)

Coverage: Transaction scenarios, service orchestration.

| Category | Tests | Status |
|----------|-------|--------|
| Invoice creation with details | 4 | CONFIRMED |
| Invoice rollback on failure | 3 | CONFIRMED |
| Bordereau creation with counter | 5 | CONFIRMED |
| Concurrent counter access | 2 | CONFIRMED |
| Workflow scenarios | 40 | CONFIRMED |
| Bordereau lifecycle | 30 | CONFIRMED |

### PostgreSQL Integration Tests (requires real CHIFA DB — MANUAL)

| Category | Tests | Status |
|----------|-------|--------|
| Real schema alignment | 50 | CONFIRMED (Phase 004.12) |
| ReadOnly validation | 30 | CONFIRMED (Phase 004.12) |
| Write validation | 3 | CONFIRMED (TST001, TST002, TST003) |
| Visibility verification | 0 (requires CHIFA UI) | PARTIAL |

### Architecture Tests

| Test | Purpose | Status |
|------|---------|--------|
| Domain → Infrastructure dependency | Must be NONE | CONFIRMED |
| Domain → Application dependency | Must be NONE | CONFIRMED |
| Application → Infrastructure dependency | Must be NONE | CONFIRMED |
| Shared → Domain dependency | Must be NONE | CONFIRMED |
| Entities inherit BaseEntity | Rule | CONFIRMED |
| Entities have parameterless constructors | Rule | CONFIRMED |
| Domain enums in Domain layer | Rule | CONFIRMED |

---

## Coverage Target

| Type | Target | Current | Status |
|------|--------|---------|--------|
| Unit tests (validation logic) | 100% | >95% | CONFIRMED |
| Integration tests (transactions) | All scenarios | ~80% | PARTIAL |
| Architecture tests (layer boundaries) | All rules | 7/7 | CONFIRMED |
| Real PG write tests (manual) | Executed + rolled back | 3/3 | CONFIRMED |
| Real PG schema alignment | Validated | 48 tables | CONFIRMED |
| Visibility tests (CHIFA UI) | Pending | 0 automated | PARTIAL |
