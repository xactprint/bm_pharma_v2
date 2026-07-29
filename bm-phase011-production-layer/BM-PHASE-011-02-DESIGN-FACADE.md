# BM-PHASE-011-02-DESIGN-FACADE — IChifaIntegrationFacade Design

## Interface

```csharp
public interface IChifaIntegrationFacade
{
    // Core invoice operations
    Task<FacadeResult<ChifaInvoiceResult>> PrepareInvoiceAsync(ChifaInvoiceRequest, CancellationToken);
    Task<FacadeResult<ChifaWorkflowResult>> ValidateInvoiceAsync(ChifaInvoiceRequest, CancellationToken);
    Task<FacadeResult<ChifaInvoiceResult>> CreateInvoiceAsync(ChifaInvoiceRequest, CancellationToken);
    Task<FacadeResult<bool>> DeleteDraftAsync(string numFact, CancellationToken);

    // Status queries
    Task<FacadeResult<ChifaInvoiceStatusSnapshot>> LoadInvoiceAsync(string numFact, CancellationToken);
    Task<FacadeResult<ChifaInvoiceStatusSnapshot>> GetInvoiceStatusAsync(string numFact, CancellationToken);
    Task<FacadeResult<ChifaBordereauStatusSnapshot>> GetBordereauStatusAsync(string numBord, CancellationToken);
    Task<FacadeResult<ChifaStatusSnapshot>> RefreshStatusAsync(CancellationToken);
    Task<FacadeResult<DashboardOverview>> GetDashboardOverviewAsync(CancellationToken);
    Task<bool> IsAvailableAsync(CancellationToken);

    // Health & status
    Task<ChifaHealthStatus> GetHealthStatusAsync(CancellationToken);
    Task<(bool isPresent, string label, bool isValid)> GetTokenStatusAsync(CancellationToken);
    Task<ChifaSigningStatus> GetSigningStatusAsync(CancellationToken);
    Task<ChifaIntegrationMode> GetModeAsync(CancellationToken);

    // Workflow
    Task<FacadeResult<ChifaWorkflowResult>> ExecuteFullWorkflowAsync(ChifaInvoiceRequest, CancellationToken);
    Task<List<ChifaWorkflowAuditEntry>> GetWorkflowAuditLogAsync(int maxEntries, CancellationToken);

    // Bordereau operations (delegated to CHIFA-OFFICINE per Option D)
    Task<List<BordereauWorkflowSummary>> GetAllBordereauxAsync(CancellationToken);
    Task<List<BordereauWorkflowAuditEntry>> GetBordereauAuditLogAsync(string? numBord, CancellationToken);
    Task<FacadeResult<BordereauWorkflowResult>> CreateBordereauAsync(string, string, List<string>, CancellationToken);
    Task<FacadeResult<BordereauWorkflowResult>> ValidateBordereauAsync(string, CancellationToken);
    Task<FacadeResult<BordereauWorkflowResult>> SignBordereauAsync(string, CancellationToken);
    Task<FacadeResult<BordereauWorkflowResult>> CloseBordereauAsync(string, CancellationToken);
    Task<FacadeResult<BordereauWorkflowResult>> TransmitBordereauAsync(string, CancellationToken);
}
```

## FacadeResult Pattern

```csharp
public enum FacadeOperationStatus { Success, ValidationFailed, WriteFailed, NotAvailable, NotFound, Conflict }

public class FacadeResult<T>
{
    public FacadeOperationStatus Status { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccess => Status == FacadeOperationStatus.Success;
}
```

## Delegation Map

| Facade Method | Delegates To |
|---|---|
| PrepareInvoiceAsync | IChifaInvoiceWorkflowService.PrepareInvoiceAsync |
| ValidateInvoiceAsync | IChifaInvoiceWorkflowService.ValidateOnlyAsync |
| CreateInvoiceAsync | IChifaInvoiceService.CreateInvoiceAsync |
| GetHealthStatusAsync | IChifaIntegrationService.GetHealthStatusAsync |
| GetTokenStatusAsync | IChifaTokenService.IsTokenPresentAsync + GetTokenInfoAsync |
| GetSigningStatusAsync | IChifaSigningService.GetSigningStatusAsync |
| ExecuteFullWorkflowAsync | IChifaInvoiceWorkflowService.ExecuteFullWorkflowAsync |
| CreateBordereauAsync | IBordereauStatusService.CreateBordereauAsync |
| ValidateBordereauAsync | IBordereauStatusService.ValidateBordereauAsync |
| SignBordereauAsync | IBordereauStatusService.SignBordereauAsync |
| CloseBordereauAsync | IBordereauStatusService.CloseBordereauAsync |
| TransmitBordereauAsync | IBordereauStatusService.TransmitBordereauAsync |
| StatusEngine operations | StatusEngine.EvaluateAsync |
| Synchronization | InvoiceSynchronizer + BordereauSynchronizer |

## Cross-Cutting Concerns

- **CorrelationContext**: Each operation logs with correlationId via `_correlation.GetOrCreate()`
- **Audit**: Every operation logged via `IChifaAuditService.LogOperationAsync`
- **Metrics**: CreateInvoiceAsync and HealthCheck recorded via `ChifaMetricsService`
- **Error handling**: All exceptions caught, logged, wrapped in `FacadeResult<T>`
