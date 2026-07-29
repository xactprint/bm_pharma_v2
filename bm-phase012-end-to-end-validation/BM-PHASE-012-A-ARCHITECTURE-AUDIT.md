# BM-PHASE-012-A — Architecture Audit

## Dependency Graph

```
Domain ← Application ← { Infrastructure, Persistence.*, Sync, CHIFA, UI }
Domain ← { CNAS, Notifications, Reporting }
Shared ← (no incoming refs)
```

**Circular dependencies: NONE** — graph is acyclic.

## Interface Implementation Audit

| Interface | Implementations | DI Registered | Status |
|---|---|---|---|
| `IChifaIntegrationService` | ChifaIntegrationServiceStub, FakeChifaIntegrationProvider | ✅ | OK |
| `IChifaInvoiceService` | ChifaPostgresInvoiceService, FakeChifaIntegrationProvider, ChifaInvoiceServiceStub | ✅ (postgres/fake) | Stub unregistered |
| `IChifaBordereauService` | ChifaPostgresBordereauService, FakeChifaIntegrationProvider, ChifaBordereauServiceStub | ✅ (postgres/fake) | Stub unregistered |
| `IChifaTokenService` | ChifaTokenServiceStub, FakeChifaIntegrationProvider | ✅ | OK |
| `IChifaSigningService` | ChifaSigningServiceStub, FakeChifaIntegrationProvider | ✅ | OK |
| `IChifaAuditService` | ChifaAuditService, StructuredChifaAuditService | ✅ (ChifaAuditService only) | Structured unregistered |
| `IChifaNumberingService` | ChifaNumberingService, ChifaNumberingServiceFake | ✅ (mode-switched) | OK |
| `IBordereauStatusService` | BordereauStatusService | ✅ | OK |
| `IChifaInvoiceWorkflowService` | ChifaInvoiceWorkflowService | ✅ | OK |
| `IChifaIntegrationFacade` | ChifaIntegrationFacade | ✅ | OK |

## Interfaces WITHOUT Implementation

| Interface | Location | Impact |
|---|---|---|
| `ICurrentUserService` | Application.Interfaces | Low — no code depends on it |
| `ILicenseService` | Application.Interfaces | Low — no code depends on it |
| `IDatabaseInitializer` | Application.Interfaces | Low — no code depends on it |
| `IReportService` | Reporting.Interfaces | Low — stub project |
| `ISyncService` | Sync.Interfaces | Low — stub project |
| `ICnasService` | CNAS.Interfaces | Low — Option D defers CNAS to CHIFA-OFFICINE |
| `IUnitOfWork` | Domain.Interfaces | Low — never adopted |
| `IRepository<T>` | Domain.Interfaces | Low — never adopted |

**Impact**: None — all are unused stubs.

## Classes Missing DI Registration

| Class | Reason | Impact |
|---|---|---|
| `OneActionWorkflowService` | Alternative workflow orchestrator | Low — tests only |
| `StructuredChifaAuditService` | Alternative audit implementer | Low — tests only |
| `ChifaInvoiceServiceStub` | Legacy stub | Low — tests only |
| `ChifaBordereauServiceStub` | Legacy stub | Low — tests only |

## CRITICAL: Duplicate Type Definitions

### `ChifaValidationError` — DUPLICATE in same namespace
- `ChifaInvoiceValidator.cs:118` → `public class ChifaValidationError` (namespace `BMPharma.CHIFA.Services`)
- `ChifaExceptions.cs:17` → `public class ChifaValidationError` (namespace `BMPharma.CHIFA.Services`)

**This will cause a CS0101 compilation error.** The version in `ChifaExceptions.cs` should be removed, since the `ChifaInvoiceValidator.cs` version is the original.

### `ChifaConcurrencyException` — DUPLICATE in different namespaces
- `ChifaWriteResult.cs:24` → `BMPharma.CHIFA.Interfaces`
- `ChifaExceptions.cs:32` → `BMPharma.CHIFA.Services`

Naming collision — should be consolidated.

## Namespace Inconsistency

`ChifaNumberingService.cs` is in `Services/` folder but declares `namespace BMPharma.CHIFA.Interfaces`. Should be `BMPharma.CHIFA.Services`.

## ViewModel Injection Audit ✅

All 3 ViewModels inject only `IChifaIntegrationFacade` + `ChifaIntegrationModeProvider` + `ILogger<T>`. No direct low-level service dependencies.

## MainViewModel Dual Constructors

Parameterless constructor and DI constructor. Registration uses `AddTransient<MainViewModel>()` which resolves via parameterless → DI is bypassed. `IModeProvider` will be null.

## Projects Without DI Registration

- `BMPharma.Application`, `Infrastructure`, `Persistence.PostgreSQL`, `Persistence.SQLite`, `CNAS`, `Sync`, `Reporting`, `Shared` — all lack extension methods. DbContexts registered manually in CHIFA and UI projects.

## Dead Projects

`BMPharma.Application`, `BMPharma.Infrastructure`, `BMPharma.Reporting`, `BMPharma.Sync`, `BMPharma.CNAS` — contain only interface files with no implementations. No functional impact.
