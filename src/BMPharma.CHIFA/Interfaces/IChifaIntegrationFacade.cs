using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Interfaces;

public enum FacadeOperationStatus
{
    Success,
    ValidationFailed,
    WriteFailed,
    NotAvailable,
    NotFound,
    Conflict
}

public class FacadeResult<T>
{
    public FacadeOperationStatus Status { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }

    public bool IsSuccess => Status == FacadeOperationStatus.Success;
}

public class DashboardOverview
{
    public ChifaStatusSnapshot Status { get; set; } = new();
    public int TotalInvoicesToday { get; set; }
    public int PendingBordereaux { get; set; }
    public string? LastOperation { get; set; }
    public ChifaSyncSummary? LastSync { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? CircuitBreakerState { get; set; }
    public string? CircuitBreakerKey { get; set; }
    public int CircuitBreakerFailureCount { get; set; }
    public int MetricsTotal { get; set; }
    public int MetricsSuccess { get; set; }
    public int MetricsFailed { get; set; }
    public double MetricsAvgMs { get; set; }
    public string? CorrelationId { get; set; }
    public string? LastSyncResult { get; set; }
    public DateTime? LastSyncTime { get; set; }
}

public interface IChifaIntegrationFacade
{
    Task<FacadeResult<ChifaInvoiceResult>> PrepareInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default);

    Task<FacadeResult<ChifaWorkflowResult>> ValidateInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default);

    Task<FacadeResult<ChifaInvoiceResult>> CreateInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default);

    Task<FacadeResult<bool>> DeleteDraftAsync(
        string numFact, CancellationToken ct = default);

    Task<FacadeResult<ChifaInvoiceStatusSnapshot>> LoadInvoiceAsync(
        string numFact, CancellationToken ct = default);

    Task<FacadeResult<ChifaInvoiceStatusSnapshot>> GetInvoiceStatusAsync(
        string numFact, CancellationToken ct = default);

    Task<FacadeResult<ChifaBordereauStatusSnapshot>> GetBordereauStatusAsync(
        string numBord, CancellationToken ct = default);

    Task<FacadeResult<ChifaSyncSummary>> SynchronizeAsync(
        CancellationToken ct = default);

    Task<FacadeResult<ChifaStatusSnapshot>> RefreshStatusAsync(
        CancellationToken ct = default);

    Task<FacadeResult<DashboardOverview>> GetDashboardOverviewAsync(
        CancellationToken ct = default);

    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    Task<ChifaHealthStatus> GetHealthStatusAsync(CancellationToken ct = default);

    Task<(bool isPresent, string label, bool isValid)> GetTokenStatusAsync(CancellationToken ct = default);

    Task<ChifaSigningStatus> GetSigningStatusAsync(CancellationToken ct = default);

    Task<ChifaIntegrationMode> GetModeAsync(CancellationToken ct = default);

    Task<FacadeResult<ChifaWorkflowResult>> ExecuteFullWorkflowAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default);

    Task<List<ChifaWorkflowAuditEntry>> GetWorkflowAuditLogAsync(
        int maxEntries = 20, CancellationToken ct = default);

    Task<List<BordereauWorkflowSummary>> GetAllBordereauxAsync(CancellationToken ct = default);

    Task<List<BordereauWorkflowAuditEntry>> GetBordereauAuditLogAsync(
        string? numBord = null, CancellationToken ct = default);

    Task<FacadeResult<BordereauWorkflowResult>> CreateBordereauAsync(
        string numBord, string codeCentre, List<string> invoiceNumbers, CancellationToken ct = default);

    Task<FacadeResult<BordereauWorkflowResult>> ValidateBordereauAsync(
        string numBord, CancellationToken ct = default);

    Task<FacadeResult<BordereauWorkflowResult>> SignBordereauAsync(
        string numBord, CancellationToken ct = default);

    Task<FacadeResult<BordereauWorkflowResult>> CloseBordereauAsync(
        string numBord, CancellationToken ct = default);

    Task<FacadeResult<BordereauWorkflowResult>> TransmitBordereauAsync(
        string numBord, CancellationToken ct = default);
}
