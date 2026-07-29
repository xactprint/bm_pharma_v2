# BM-PHASE-010 — SECURITY ARCHITECTURE

**Version:** 2.0 (Phase 010 reconciliation)
**Date:** 2026-07-29
**Status:** ACTIVE
**Source of Truth:** Phase 004.8 (security report), Phase 004.12 (ReadOnly validation), Phase 009-C (credentials in memory)
**Previous Version:** BM_PHARMA_CHIFA_SECURITY.md v1.0 (2026-07-25)

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-07-25 | BM Pharma | Initial security contract |
| 2.0 | 2026-07-29 | BM Pharma | Reconciled with credential leakage discovery, real security validation |

### Major Changes from v1.0

| Change | v1.0 (Hypothesis) | v2.0 (Real) | Source |
|--------|-------------------|-------------|--------|
| Credential safety | Assumed safe | CONFIRMED plaintext credentials in CHIFA process memory | Phase 009-C |
| SQL visibility | Assumed compiled | CONFIRMED plaintext SQL in process memory | Phase 009-C |
| Trust auth | Documented | CONFIRMED — no password required | Phase 004.12 |
| 3-layer ReadOnly | Documented | CONFIRMED working against real PG | Phase 004.12 |
| Token data in parametre | Suspected | PARTIAL — tokens may be in plaintext in parametre table | Phase 004.9 |

---

## Integration Modes

| Mode | SELECT | INSERT | UPDATE | DELETE | Use Case | Status |
|------|--------|--------|--------|--------|----------|--------|
| ReadOnly | Yes | No | No | No | Default. Safe. | CONFIRMED |
| Test | Yes | Yes | Yes | Yes | Testing only. Synthetic data. | CONFIRMED |
| Production | Yes | Yes | Yes | Yes | Protected. Audit required. | CONFIRMED |

### Default: ReadOnly (CONFIRMED)

BM Pharma ships in ReadOnly mode. All CHIFA writes are blocked by default.

To enable writes, user must:
1. Open Settings → CHIFA Integration
2. Change mode from ReadOnly to Test or Production
3. Enter admin password
4. Acknowledge warning dialog

---

## 3-Layer ReadOnly Protection (CONFIRMED)

| Layer | Mechanism | Location | Status |
|-------|-----------|----------|--------|
| 1. DI Wiring | ReadOnly mode registers FakeChifaIntegrationProvider instead of real Postgres services | DependencyInjection.cs | CONFIRMED |
| 2. ChifaWriteGuard | Runtime guard throws ChifaWriteBlockedException if mode is ReadOnly | ChifaWriteGuard.cs | CONFIRMED |
| 3. UI | Dashboard hides "ACTION REQUISE" panel in ReadOnly mode | ChifaDashboardView.xaml | CONFIRMED |

### Layer 1: DI Wiring

```csharp
// ReadOnly mode: FakeChifaIntegrationProvider for all CHIFA services
services.AddSingleton<FakeChifaIntegrationProvider>();
services.AddScoped<IChifaInvoiceService>(sp => sp.GetRequiredService<FakeChifaIntegrationProvider>());
// ... all other CHIFA interfaces mapped to Fake provider
```

### Layer 2: ChifaWriteGuard

```csharp
public static class ChifaWriteGuard
{
    public static async Task EnsureWriteAllowedAsync(
        Func<Task<ChifaIntegrationMode>> modeProvider)
    {
        var mode = await modeProvider();
        if (mode == ChifaIntegrationMode.ReadOnly)
            throw new ChifaWriteBlockedException(
                "CHIFA integration is in ReadOnly mode");
    }
}
```

### Layer 3: UI

- Dashboard status cards show "ReadOnly" explicitly
- Action buttons disabled in ReadOnly mode
- "Action Requise" panel hidden

---

## Identified Risks

### Risk 1: PostgreSQL 9.3.4 EOL

| Aspect | Detail |
|--------|--------|
| Risk | No security patches since 2018 |
| Impact | Vulnerabilities unpatched |
| Mitigation | Accept for embedded localhost use |
| Status | CONFIRMED |

### Risk 2: Trust Authentication

| Aspect | Detail |
|--------|--------|
| Risk | No password required for database access |
| Impact | Any local process can connect |
| Mitigation | Restricted to localhost (127.0.0.1) |
| Status | CONFIRMED |

### Risk 3: Credentials in CHIFA Process Memory

| Aspect | Detail |
|--------|--------|
| Risk | Plaintext PostgreSQL credentials visible in CHIFA process memory | |
| Discovery | Windbg memory scan on CHIFA_OFFICINE.exe (Phase 009-C) |
| Evidence | `Server=.;Port=5432;User Id=stock;Password=stock;Database=STOCK;` visible in process memory |
| Impact | Any user with process access can read credentials |
| Mitigation | Accept for single-pharmacist workstation; document as known risk |
| Status | CONFIRMED |

### Risk 4: SQL Queries in Process Memory

| Aspect | Detail |
|--------|--------|
| Risk | All SQL queries visible in plaintext in CHIFA process memory |
| Evidence | 4 SQL families found via Windbg memory scan (Phase 009-C) |
| Impact | Reverse engineering possible |
| Status | CONFIRMED |

### Risk 5: Token Data in parametre Table

| Aspect | Detail |
|--------|--------|
| Risk | Token values may be stored in plaintext in parametre table |
| Evidence | Phase 004.9 discovered 58 columns, some potentially containing token/crypto material |
| Impact | Token exposure if database accessed |
| Mitigation | Read-only access to parametre; do not expose in logs or UI |
| Status | PARTIAL |

### Risk 6: Superuser `pharm`

| Aspect | Detail |
|--------|--------|
| Risk | BM Pharma uses superuser account (pharm) for database access |
| Impact | Full database access, not restricted to specific tables |
| Mitigation | Application-level restrictions (WriteGuard, validation) compensate |
| Status | CONFIRMED |

---

## What BM Pharma MUST NOT Do

| Rule | Rationale | Status |
|------|-----------|--------|
| 1. Modify CHIFA-OFFICINE binary | Legal/compatibility risk | CONFIRMED |
| 2. Patch CHIFA DLLs | Legal/compatibility risk | CONFIRMED |
| 3. Bypass token authentication | Regulatory requirement | CONFIRMED |
| 4. Create fake signatures | Regulatory violation | CONFIRMED |
| 5. Automate signing operations | Regulatory violation | CONFIRMED |
| 6. Send directly to CNAS without CHIFA | Regulatory violation | CONFIRMED |
| 7. Delete CHIFA data | Data integrity | CONFIRMED |
| 8. Modify existing CHIFA records (except counter + invoice creation) | Data integrity | CONFIRMED |

---

## Safety Guards

| Guard | Implementation | Verified | Status |
|-------|---------------|----------|--------|
| 1. Write blocking | ChifaWriteGuard throws exception in ReadOnly mode | Phase 004.12 | CONFIRMED |
| 2. Confirmation dialog | Show write details before execution | Architecture | CONFIRMED |
| 3. Audit trail | Every operation logged (timestamp, user, operation, table, key, result, duration, correlationId) | Phase 007 | CONFIRMED |
| 4. No private key access | BM Pharma never accesses token, PIN, or signs documents | Phase 008 | CONFIRMED |
| 5. No credential logging | Audit service explicitly excludes connection strings, passwords | Phase 004.7 | CONFIRMED |

---

## Security Certitude Matrix

| Element | Status | Source |
|---------|--------|--------|
| ReadOnly default mode | CONFIRMED | Phase 004.12 |
| 3-layer protection works | CONFIRMED | Phase 004.12 |
| ChifaWriteGuard blocks writes | CONFIRMED | 509 tests |
| Credentials in CHIFA memory | CONFIRMED | Phase 009-C |
| SQL in CHIFA memory | CONFIRMED | Phase 009-C |
| Trust auth (no password) | CONFIRMED | Phase 004.12 |
| Superuser access (pharm) | CONFIRMED | Phase 004.11 |
| Token data in parametre | PARTIAL | Phase 004.9 |
| No credential in logs | CONFIRMED | Phase 004.7 |
| RBAC implemented | NOT IMPLEMENTED | Documented limitation |
| AdminPassword externalized | NOT IMPLEMENTED | Currently hardcoded in appsettings |
