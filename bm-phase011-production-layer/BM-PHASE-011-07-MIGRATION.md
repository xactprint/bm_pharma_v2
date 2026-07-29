# BM-PHASE-011-07-MIGRATION — Migration Path

## Migration Steps

### Step 1: Register new services
Add to `DependencyInjection.cs`:
```csharp
services.AddSingleton<CorrelationContext>();
services.AddSingleton<ChifaCircuitBreaker>();
services.AddSingleton<ChifaMetricsService>();
services.AddScoped<ChifaHealthCheckService>();
services.AddScoped<StatusEngine>();
services.AddScoped<InvoiceSynchronizer>();
services.AddScoped<BordereauSynchronizer>();
services.AddScoped<StatusSynchronizer>();
services.AddScoped<ChifaMonitoringService>();
services.AddScoped<IChifaIntegrationFacade, ChifaIntegrationFacade>();
```

### Step 2: Refactor ViewModels

**Before (ChifaDashboardViewModel):**
```csharp
public ChifaDashboardViewModel(
    IChifaIntegrationService integrationService,
    IChifaTokenService tokenService,
    IChifaSigningService signingService,
    ChifaIntegrationModeProvider modeProvider,
    ILogger<ChifaDashboardViewModel> logger)
```

**After:**
```csharp
public ChifaDashboardViewModel(
    IChifaIntegrationFacade facade,
    ChifaIntegrationModeProvider modeProvider,
    ILogger<ChifaDashboardViewModel> logger)
```

### Step 3: ViewModel Method Replacement

| Old Call | New Call |
|---|---|
| `_integrationService.IsChifaAvailableAsync()` | `_facade.IsAvailableAsync()` |
| `_integrationService.GetHealthStatusAsync()` | `_facade.GetHealthStatusAsync()` |
| `_tokenService.IsTokenPresentAsync()` | `_facade.GetTokenStatusAsync()` |
| `_tokenService.GetTokenInfoAsync()` | `_facade.GetTokenStatusAsync()` (combined) |
| `_signingService.GetSigningStatusAsync("_global")` | `_facade.GetSigningStatusAsync()` |
| `_workflowService.ValidateOnlyAsync(request)` | `_facade.ValidateInvoiceAsync(request)` |
| `_workflowService.ExecuteFullWorkflowAsync(request)` | `_facade.ExecuteFullWorkflowAsync(request)` |
| `_workflowService.GetAuditLog()` | `_facade.GetWorkflowAuditLogAsync()` |
| `_bordereauService.GetAllBordereaux()` | `_facade.GetAllBordereauxAsync()` |
| `_bordereauService.GetAuditLog(numBord)` | `_facade.GetBordereauAuditLogAsync(numBord)` |
| `_bordereauService.CreateBordereauAsync(...)` | `_facade.CreateBordereauAsync(...)` |
| `_bordereauService.ValidateBordereauAsync(...)` | `_facade.ValidateBordereauAsync(...)` |
| `_bordereauService.SignBordereauAsync(...)` | `_facade.SignBordereauAsync(...)` |
| `_bordereauService.CloseBordereauAsync(...)` | `_facade.CloseBordereauAsync(...)` |
| `_bordereauService.TransmitBordereauAsync(...)` | `_facade.TransmitBordereauAsync(...)` |

### Step 4: Existing Services Preserved

All existing services remain registered for backward compatibility:
- `FakeChifaIntegrationProvider` still implements 5 interfaces
- `ChifaPostgresInvoiceService` still writes via `ChifaWriteDbContext`
- `ChifaPostgresBordereauService` still handles bordereau reads
- `ChifaInvoiceWorkflowService` still orchestrates workflows
- `BordereauStatusService` still manages bordereau tracking

The facade delegates TO these services — it does not replace them.
