# BM-PHASE-010 — SERVICE ARCHITECTURE & DEPENDENCY INJECTION

**Version:** 1.0 (Phase 010 creation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phases 001-003 (initial architecture), Phase 006 (EF Core integration), Phase 004.12 (real PG validation)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-29 | BM Pharma | Initial service architecture document |

---

## Service Layer Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           BM Pharma (.NET 8)                             │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                     Application Layer                              │  │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────────┐ │  │
│  │  │ MediatR Commands │  │ MediatR Queries  │  │ Domain Events     │ │  │
│  │  └────────┬────────┘  └────────┬────────┘  └────────┬──────────┘ │  │
│  └───────────┼─────────────────────┼─────────────────────┼────────────┘  │
│              │                     │                     │               │
│  ┌───────────▼─────────────────────▼─────────────────────▼────────────┐  │
│  │                      CHIFA Integration Layer                       │  │
│  │                                                                     │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │                  CHIFA Services                               │   │  │
│  │  │  ┌─────────────────────────────────────────────────────┐    │   │  │
│  │  │  │ ChifaPostgresInvoiceService   (REAL — Phase 007)    │    │   │  │
│  │  │  │ ChifaPostgresBordereauService (REAL — Phase 006)    │    │   │  │
│  │  │  │ ChifaNumberingService         (REAL — Phase 006)    │    │   │  │
│  │  │  │ ChifaAuditService             (REAL — Phase 002)    │    │   │  │
│  │  │  │ ChifaInvoiceValidator         (REAL — Phase 002)    │    │   │  │
│  │  │  │ ChifaBordereauValidator       (REAL — Phase 002)    │    │   │  │
│  │  │  │ ChifaInvoiceMapper            (REAL — Phase 003)    │    │   │  │
│  │  │  │ ChifaBordereauMapper          (REAL — Phase 003)    │    │   │  │
│  │  │  └─────────────────────────────────────────────────────┘    │   │  │
│  │  │                                                             │   │  │
│  │  │  ┌─────────────────────────────────────────────────────┐    │   │  │
│  │  │  │ Workflow Services (Simulated in ReadOnly)           │    │   │  │
│  │  │  │ ChifaInvoiceWorkflowService  (REAL — Phase 004.5)   │    │   │  │
│  │  │  │ BordereauStatusService        (REAL — Phase 004.6)  │    │   │  │
│  │  │  │ BordereauWorkflowStateMachine (REAL — Phase 004.6)  │    │   │  │
│  │  │  └─────────────────────────────────────────────────────┘    │   │  │
│  │  │                                                             │   │  │
│  │  │  ┌─────────────────────────────────────────────────────┐    │   │  │
│  │  │  │ Simulation Provider (Default in ReadOnly)           │    │   │  │
│  │  │  │ FakeChifaIntegrationProvider (REAL — Phase 003)     │    │   │  │
│  │  │  └─────────────────────────────────────────────────────┘    │   │  │
│  │  │                                                             │   │  │
│  │  │  ┌─────────────────────────────────────────────────────┐    │   │  │
│  │  │  │ Guards & Configuration                              │    │   │  │
│  │  │  │ ChifaWriteGuard          (REAL — Phase 002)         │    │   │  │
│  │  │  │ ChifaIntegrationConfig   (REAL — Phase 002)         │    │   │  │
│  │  │  │ ChifaIntegrationModeProvider (REAL — Phase 002)     │    │   │  │
│  │  │  └─────────────────────────────────────────────────────┘    │   │  │
│  │  └─────────────────────────────────────────────────────────────┘   │  │
│  │                                                                     │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │                  CHIFA Interfaces (Contracts)                │   │  │
│  │  │  IChifaIntegrationService       — Health check               │   │  │
│  │  │  IChifaInvoiceService           — Invoice CRUD in CHIFA      │   │  │
│  │  │  IChifaBordereauService         — Bordereau CRUD in CHIFA    │   │  │
│  │  │  IChifaTokenService             — Token detection            │   │  │
│  │  │  IChifaSigningService           — Signing status             │   │  │
│  │  │  IChifaAuditService             — Audit logging              │   │  │
│  │  │  IChifaNumberingService         — Atomic counters            │   │  │
│  │  │  IChifaInvoiceWorkflowService   — Workflow orchestration     │   │  │
│  │  │  IBordereauStatusService        — Bordereau monitoring       │   │  │
│  │  └─────────────────────────────────────────────────────────────┘   │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                   Persistence Layer                               │   │
│  │  ┌─────────────────────────┐  ┌──────────────────────────────┐   │   │
│  │  │ Persistence.SQLite      │  │ Persistence.PostgreSQL       │   │   │
│  │  │ (EF Core SQLite)        │  │ (EF Core Npgsql 8.x)         │   │   │
│  │  │ — BM Pharma local data  │  │ — CHIFA_OFFICINE read/write  │   │   │
│  │  └─────────────────────────┘  └──────────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Interface Contracts

| Interface | Methods | Real Implementation | Status |
|-----------|---------|-------------------|--------|
| IChifaIntegrationService | HealthCheckAsync, GetModeAsync | ChifaPostgreSqlContext | CONFIRMED |
| IChifaInvoiceService | CreateInvoiceAsync, InvoiceExistsAsync | ChifaPostgresInvoiceService | CONFIRMED |
| IChifaBordereauService | CreateBordereauAsync, GetNextNumberAsync, SignBordereauAsync, CloseBordereauAsync | ChifaPostgresBordereauService | PARTIAL (sign/close = delegated) |
| IChifaTokenService | IsTokenAvailableAsync, GetTokenInfoAsync | ChifaSigningServiceStub | CONFIRMED (always returns false) |
| IChifaSigningService | GetSigningStatusAsync | ChifaSigningServiceStub | CONFIRMED (always returns NotSigned) |
| IChifaAuditService | LogOperationAsync, GetAuditLogAsync | ChifaAuditService | CONFIRMED |
| IChifaNumberingService | GetNextInvoiceNumberAsync, GetNextBordereauNumberAsync, PeekNextInvoiceNumberAsync, PeekNextBordereauNumberAsync | ChifaNumberingService | CONFIRMED |
| IChifaInvoiceWorkflowService | ExecuteFullWorkflowAsync, ValidateOnlyAsync, PrepareInvoiceAsync, CreateInDatabaseAsync, CheckVisibilityAsync, SignBordereauAsync, CloseBordereauAsync, AssignBordereauAsync | ChifaInvoiceWorkflowService | CONFIRMED |
| IBordereauStatusService | CreateBordereauAsync, AttachInvoicesAsync, RemoveInvoiceAsync, ValidateBordereauAsync, SignBordereauAsync, CloseBordereauAsync, TransmitBordereauAsync, TransitionToAsync, GetStatusAsync, GetAllBordereauxAsync | BordereauStatusService | CONFIRMED |

---

## Dependency Injection Wiring

### ReadOnly Mode (Default)

In ReadOnly mode, all CHIFA services are backed by FakeChifaIntegrationProvider:

```csharp
// Simulation provider (singleton)
services.AddSingleton<FakeChifaIntegrationProvider>();

// All CHIFA interfaces → Fake provider
services.AddScoped<IChifaIntegrationService>(
    sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
services.AddScoped<IChifaInvoiceService>(
    sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
services.AddScoped<IChifaBordereauService>(
    sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
services.AddScoped<IChifaTokenService>(
    sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
services.AddScoped<IChifaSigningService>(
    sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());

// Workflow services (use injected interfaces → Fake provider)
services.AddScoped<IChifaInvoiceWorkflowService, ChifaInvoiceWorkflowService>();
services.AddScoped<IBordereauStatusService, BordereauStatusService>();
services.AddScoped<BordereauWorkflowStateMachine>();

// Real services (registered but not wired to interfaces)
services.AddScoped<ChifaPostgresInvoiceService>();
services.AddScoped<ChifaPostgresBordereauService>();
services.AddScoped<ChifaNumberingService>();
services.AddScoped<ChifaAuditService>();
```

### Test/Production Mode (Write-Enabled)

In Test or Production mode, real Postgres services replace the fake provider:

```csharp
// Real Postgres services
services.AddScoped<IChifaInvoiceService, ChifaPostgresInvoiceService>();
services.AddScoped<IChifaBordereauService, ChifaPostgresBordereauService>();
services.AddScoped<IChifaTokenService, ChifaSigningServiceStub>(); // Still stubs
services.AddScoped<IChifaSigningService, ChifaSigningServiceStub>(); // Still stubs
services.AddScoped<IChifaIntegrationService, ChifaPostgreSqlContext>();
services.AddScoped<IChifaNumberingService, ChifaNumberingService>();
services.AddScoped<IChifaAuditService, ChifaAuditService>();
```

---

## DbContext Architecture

```
ChifaPostgreSqlContext (READ-ONLY queries)
    └─ 6 entities: ChifaFacture, ChifaDetailFact, ChifaBordereau,
                   ChifaParametre, ChifaMedicament, ChifaSignature
    └─ Configurations: ChifaFactureConfiguration (shared),
                       ChifaDetailFactConfiguration (shared),
                       ChifaBordereauConfiguration (shared),
                       ChifaParametreConfiguration (shared),
                       ChifaMedicamentConfiguration (shared),
                       ChifaSignatureConfiguration (shared)

ChifaWriteDbContext (WRITE operations)
    └─ Same 6 entities + same 6 shared configurations
    └─ Additional Fluent API for write-specific behavior
```

### Configuration Sharing

Entity configurations are extracted to separate files and shared between both DbContexts via `ApplyConfigurationsFromAssembly`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(ChifaFactureConfiguration).Assembly);
}
```

This eliminates duplication (previously 180 lines in each context, now 20 lines each).

---

## Separation of Responsibilities

| Service | Read | Write | Depends On |
|---------|------|-------|------------|
| ChifaPostgresInvoiceService | InvoiceExistsAsync | CreateInvoiceAsync | ChifaWriteDbContext, ChifaWriteGuard, ChifaInvoiceValidator, IChifaAuditService |
| ChifaPostgresBordereauService | GetNextNumberAsync | CreateBordereauAsync | ChifaWriteDbContext, ChifaWriteGuard, ChifaBordereauValidator, IChifaAuditService, IChifaNumberingService |
| ChifaNumberingService | PeekNext*Async | GetNext*Async | ChifaWriteDbContext, ChifaWriteGuard |
| ChifaAuditService | GetAuditLogAsync | LogOperationAsync | ChifaWriteDbContext |
| ChifaInvoiceWorkflowService | All workflow checks | Full pipeline | IChifaInvoiceService, IChifaBordereauService, IChifaTokenService, IChifaSigningService, IChifaAuditService |
| BordereauStatusService | GetAllBordereauxAsync, GetStatusAsync | Full lifecycle | IChifaBordereauService, IChifaTokenService, IChifaSigningService, BordereauWorkflowStateMachine, IChifaAuditService |

---

## Service Certitude Matrix

| Service | Status | Source |
|---------|--------|--------|
| ChifaPostgresInvoiceService | CONFIRMED (real write validated) | Phase 007 |
| ChifaPostgresBordereauService | PARTIAL (InMemory only) | Phase 006 |
| ChifaNumberingService | CONFIRMED (atomic counters) | Phase 006 |
| ChifaAuditService | CONFIRMED | Phase 002 |
| ChifaInvoiceValidator | CONFIRMED | Phase 002 |
| ChifaBordereauValidator | CONFIRMED | Phase 002 |
| ChifaWriteGuard | CONFIRMED | Phase 004.12 |
| FakeChifaIntegrationProvider | CONFIRMED | Phase 003 |
| ChifaInvoiceWorkflowService | CONFIRMED | Phase 004.5 |
| BordereauStatusService | CONFIRMED | Phase 004.6 |
| ChifaInvoiceMapper | CONFIRMED | Phase 003 |
| ChifaBordereauMapper | CONFIRMED | Phase 003 |
