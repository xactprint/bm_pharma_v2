# BM-PHASE-003 — CODE AUDIT

**Date:** 2025-07-25
**Status:** READ-ONLY — No code modified during this audit

---

## 1. SOLUTION STRUCTURE

| Category | Count |
|----------|-------|
| Solution files | 1 |
| Source projects | 12 |
| Tool projects | 2 |
| Test projects | 6 (2 empty) |
| Total .csproj | 20 |

### Projects

| Project | Role | Status |
|---------|------|--------|
| BMPharma.Domain | Entities, Enums, Interfaces | COMPLETE |
| BMPharma.Application | CQRS, Validators | BASIC |
| BMPharma.Infrastructure | Cross-cutting | SHELL |
| BMPharma.Persistence.SQLite | SQLite DbContext + Configs | COMPLETE |
| BMPharma.Persistence.PostgreSQL | PostgreSQL DbContext | SKELETON |
| BMPharma.Shared | Result, Constants | COMPLETE |
| BMPharma.CHIFA | CHIFA Integration | PARTIAL |
| BMPharma.CNAS | CNAS Service | SHELL |
| BMPharma.Sync | Sync Service | SHELL |
| BMPharma.Notifications | Notifications | STUB |
| BMPharma.Reporting | Reports | SHELL |
| BMPharma.UI | WPF Application | BASIC |

---

## 2. CHIFA SERVICES — DETAILED STATUS

### 2.1 Interfaces (6 files) — ALL DEFINED

| Interface | File | Status |
|-----------|------|--------|
| IChifaAuditService | Interfaces/IChifaAuditService.cs | DEFINED |
| IChifaBordereauService | Interfaces/IChifaBordereauService.cs | DEFINED |
| IChifaIntegrationService | Interfaces/IChifaIntegrationService.cs | DEFINED |
| IChifaInvoiceService | Interfaces/IChifaInvoiceService.cs | DEFINED |
| IChifaSigningService | Interfaces/IChifaSigningService.cs | DEFINED |
| IChifaTokenService | Interfaces/IChifaTokenService.cs | DEFINED |

### 2.2 Implementations (10 files)

| Service | Type | Status | Notes |
|---------|------|--------|-------|
| ChifaAuditService | Real | PARTIAL | Logs to ILogger only. No DB persistence. No CorrelationId. No Source/Destination. |
| ChifaBordereauServiceStub | Stub | COMPLETE (as stub) | Returns false for all operations |
| ChifaBordereauValidator | Validator | COMPLETE | 4 rules: num_bord, code_centre, invoices |
| ChifaIntegrationServiceStub | Stub | COMPLETE (as stub) | Returns false/unavailable |
| ChifaInvoiceServiceStub | Stub | COMPLETE (as stub) | Returns false for all operations |
| ChifaInvoiceValidator | Validator | COMPLETE | 11 rules: num_fact, num_assure, lines, etc. |
| ChifaPostgresBordereauService | Real | PARTIAL | Guard + validation OK. Transaction empty. Counter hardcoded. No actual DB write. |
| ChifaPostgresInvoiceService | Real | PARTIAL | Guard + validation OK. Transaction empty. No actual DB write. |
| ChifaSigningServiceStub | Stub | COMPLETE (as stub) | Delegates to CHIFA-OFFICINE |
| ChifaTokenServiceStub | Stub | COMPLETE (as stub) | Returns false |

### 2.3 DI Registration

Current wiring in `DependencyInjection.cs`:
```
IChifaIntegrationService → ChifaIntegrationServiceStub    ← STUB
IChifaInvoiceService     → ChifaInvoiceServiceStub        ← STUB
IChifaBordereauService   → ChifaBordereauServiceStub      ← STUB
IChifaTokenService       → ChifaTokenServiceStub           ← STUB
IChifaSigningService     → ChifaSigningServiceStub         ← STUB
IChifaAuditService       → ChifaAuditService               ← REAL (logger only)
ChifaInvoiceValidator    → registered
ChifaBordereauValidator  → registered
ChifaWriteGuard          → registered
```

**Note:** Postgres services exist but are NOT wired. This is intentional for safety.

---

## 3. DOMAIN ENTITIES — CHIFA RELEVANCE

### 3.1 Invoice
- Has: InvoiceNumber, InvoiceDate, SubTotalDA, TaxDA, TotalDA, Status (InvoiceStatus)
- Missing: CHIFA-specific fields (num_assure, code_centre, mont_as, mont_mut, echifa, etc.)
- Missing: ChifaOperationStatus tracking

### 3.2 InvoiceLine
- Has: Quantity, UnitPriceDA, LineTotalDA, ProductId
- Missing: CHIFA-specific fields (medic_code, num_enr, inf_tr, applic_tr, etc.)

### 3.3 Bordereau
- Has: BordereauNumber, Status (BordereauStatus), TotalInvoices, TotalAmountDA
- Missing: CHIFA-specific fields (type_bord, date_bord, signed_at, closed_at)
- Missing: State machine transitions

### 3.4 Customer
- Has: InsuranceNumber, IsInsured, InsuranceProvider
- Maps to: num_assure in CHIFA

### 3.5 Product
- Has: Code, CIPCode, PriceDA, IsReimbursable, ReimbursementRate
- Maps to: medic_code in CHIFA (needs mapping table)

### 3.6 ChifaAuditLog
- EXISTS but not used by any service for persistence

---

## 4. ENUMS — CHIFA RELEVANCE

### 4.1 InvoiceStatus
```csharp
Draft = 0, Completed = 1, Cancelled = 2, Refunded = 3
```
**Missing:** CHIFA-specific states. Needs extension.

### 4.2 BordereauStatus
```csharp
Draft = 0, Submitted = 1, Signed = 2, Closed = 3, Rejected = 4
```
**Missing:** Transmitted state. No simulation vs confirmed distinction.

### 4.3 ChifaOperationStatus
```csharp
NotStarted = 0, InProgress = 1, DatabaseCreated = 2, CHIFAVisible = 3,
ReadyForSigning = 4, SigningInProgress = 5, Signed = 6, Closed = 7,
Transmitted = 8, Rejected = 9, Failed = 10
```
**Status:** Exists but NOT referenced by Invoice entity. Not used in workflow.

### 4.4 ChifaIntegrationMode
```csharp
ReadOnly = 0, Test = 1, Production = 2
```
**Status:** Used by ChifaWriteGuard. OK.

---

## 5. DATABASE CONTEXTS

### 5.1 BmPharmaDbContext (SQLite) — COMPLETE
- 13 DbSets configured
- 14 entity configurations
- Auto-timestamps on SaveChanges

### 5.2 ChifaPostgreSqlContext (PostgreSQL) — SKELETON
- No DbSets
- No entity configurations
- Comment says "Phase 4"

### 5.3 ChifaWriteDbContext (PostgreSQL) — SKELETON
- No DbSets
- No entity configurations
- Comment says "Phase 4"

---

## 6. UI — CHIFA DASHBOARD

### 6.1 ChifaDashboardViewModel
- Basic: ConnectionStatus, IntegrationMode, LastSyncDate, Counters
- NOT wired to any services
- RefreshStatus() is empty

### 6.2 ChifaInvoicePreparationViewModel
- Basic: InvoiceNumber, InsuranceNumber, CentreCode, CareDate
- NOT wired to any services
- No validation logic

### 6.3 Views
- ChifaDashboardView.xaml — exists
- ChifaInvoicePreparationView.xaml — exists
- ChifaBordereauStatusView.xaml — exists

---

## 7. TESTS — CURRENT STATE

### 7.1 Existing Tests (44 passing per Phase 002 report)

| Test File | Tests | Coverage |
|-----------|-------|----------|
| ChifaInvoiceValidatorTests | 11 | Validation rules |
| ChifaInvoiceServiceTests | 2 | Stub only |
| ChifaBordereauServiceTests | 4 | Stub only |
| ChifaBordereauValidatorTests | 5 | Validation rules |
| ChifaSigningServiceTests | 2 | Stub only |
| ChifaWriteGuardTests | 4 | Guard logic |
| InvoiceTests | 2 | Domain entity |
| Domain tests (User, Product) | ~8 | Domain entity |
| Application tests (Result) | ~4 | Result pattern |
| Architecture tests | ~2 | Architecture rules |

### 7.2 Missing Tests
- No mapping tests (BM Pharma → CHIFA)
- No workflow/orchestration tests
- No state machine tests
- No FakeChifaIntegrationProvider tests
- No negative scenario tests (20 required)
- No integration tests
- No audit service tests
- No counter management tests

---

## 8. WHAT MUST BE BUILT FOR PHASE 003

### 8.1 Code Gaps

| Gap | Priority | Description |
|-----|----------|-------------|
| ChifaInvoiceState enum/property | HIGH | Invoice needs CHIFA workflow state tracking |
| BM→CHIFA Mapping layer | HIGH | No mapping between BM entities and CHIFA models |
| FakeChifaIntegrationProvider | HIGH | Simulation environment for local testing |
| OneActionWorkflowService | HIGH | Orchestrator for complete workflow |
| State machine | HIGH | Transitions with guards |
| Enhanced ChifaAuditService | MEDIUM | Structured audit with all required fields |
| Enhanced Dashboard | MEDIUM | Wired to services, visual states |
| Invoice entity extension | HIGH | Add CHIFA fields |
| Counter management | HIGH | Atomic bordereau counter |
| Negative tests | HIGH | 20 scenarios required |

### 8.2 Documentation Gaps

| Document | Status |
|----------|--------|
| BM-PHASE-003-CODE-AUDIT.md | THIS FILE |
| BM-PHASE-003-WORKFLOW-STATE-MACHINE.md | TO CREATE |
| BM-PHASE-003-CHIFA-MAPPING.md | TO CREATE |
| BM-PHASE-003-MANUAL-TEST-PLAN.md | TO CREATE |
| BM-PHASE-003-ERROR-MATRIX.md | TO CREATE |
| BM-PHASE-003-INTEGRATION-REPORT.md | TO CREATE |
| BM-PHASE-003-REPORT.md | TO CREATE |

---

## 9. RISK ASSESSMENT

| Risk | Level | Mitigation |
|------|-------|------------|
| PostgreSQL schemas not defined | MEDIUM | Phase 003 uses simulation only |
| No entity mapping layer | HIGH | Build in Phase 003 |
| Stubs wired in DI | LOW | Intentional for safety |
| Empty Postgres transactions | MEDIUM | Simulation bypasses real DB |
| Audit not persisted | LOW | Logger-only is acceptable for simulation |
| No CHIFA fields on Invoice | HIGH | Extend Invoice entity |

---

## 10. CONCLUSION

BM-PHASE-002 delivered solid foundations (validators, guards, stubs, basic Postgres services). Phase 003 requires building the workflow layer ON TOP of these foundations, with a complete simulation environment. No real CHIFA writes needed.
