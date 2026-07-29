using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaIntegrationFacade : IChifaIntegrationFacade, IDisposable
{
    private readonly IChifaIntegrationService _integration;
    private readonly IChifaInvoiceService _invoice;
    private readonly IChifaBordereauService _bordereau;
    private readonly IChifaTokenService _token;
    private readonly IChifaSigningService _signing;
    private readonly IChifaAuditService _audit;
    private readonly IChifaInvoiceWorkflowService _workflow;
    private readonly IBordereauStatusService _bordereauStatus;
    private readonly ChifaWorkflowStateMachine _workflowState;
    private readonly IChifaNumberingService _numbering;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly StatusEngine _statusEngine;
    private readonly InvoiceSynchronizer _invoiceSync;
    private readonly BordereauSynchronizer _bordereauSync;
    private readonly StatusSynchronizer _statusSync;
    private readonly ChifaMonitoringService _monitoring;
    private readonly ChifaMetricsService _metrics;
    private readonly CorrelationContext _correlation;
    private readonly ILogger<ChifaIntegrationFacade> _logger;

    public ChifaIntegrationFacade(
        IChifaIntegrationService integration,
        IChifaInvoiceService invoice,
        IChifaBordereauService bordereau,
        IChifaTokenService token,
        IChifaSigningService signing,
        IChifaAuditService audit,
        IChifaInvoiceWorkflowService workflow,
        IBordereauStatusService bordereauStatus,
        ChifaWorkflowStateMachine workflowState,
        IChifaNumberingService numbering,
        ChifaIntegrationModeProvider modeProvider,
        StatusEngine statusEngine,
        InvoiceSynchronizer invoiceSync,
        BordereauSynchronizer bordereauSync,
        StatusSynchronizer statusSync,
        ChifaMonitoringService monitoring,
        ChifaMetricsService metrics,
        CorrelationContext correlation,
        ILogger<ChifaIntegrationFacade> logger)
    {
        _integration = integration;
        _invoice = invoice;
        _bordereau = bordereau;
        _token = token;
        _signing = signing;
        _audit = audit;
        _workflow = workflow;
        _bordereauStatus = bordereauStatus;
        _workflowState = workflowState;
        _numbering = numbering;
        _modeProvider = modeProvider;
        _statusEngine = statusEngine;
        _invoiceSync = invoiceSync;
        _bordereauSync = bordereauSync;
        _statusSync = statusSync;
        _monitoring = monitoring;
        _metrics = metrics;
        _correlation = correlation;
        _logger = logger;
    }

    public async Task<FacadeResult<ChifaInvoiceResult>> PrepareInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var mode = await _modeProvider.GetModeAsync().ConfigureAwait(false);
            if (mode == ChifaIntegrationMode.ReadOnly)
                return new FacadeResult<ChifaInvoiceResult>
                {
                    Status = FacadeOperationStatus.NotAvailable,
                    ErrorMessage = "Cannot prepare invoice in ReadOnly mode",
                    DurationMs = sw.ElapsedMilliseconds
                };

            var wfResult = await _workflow.PrepareInvoiceAsync(request, _correlation.Current ?? "system", ct).ConfigureAwait(false);

            await _audit.LogOperationAsync("PrepareInvoice", "ChifaFacture",
                request.NumFact, wfResult?.IsSuccess ?? false, sw.ElapsedMilliseconds,
                wfResult?.ErrorMessage, _correlation.Current, ct).ConfigureAwait(false);

            return new FacadeResult<ChifaInvoiceResult>
            {
                Status = wfResult?.IsSuccess == true ? FacadeOperationStatus.Success : FacadeOperationStatus.ValidationFailed,
                Data = wfResult != null ? new ChifaInvoiceResult
                {
                    Success = wfResult.IsSuccess,
                    ErrorMessage = wfResult.ErrorMessage,
                    ChifaNumFact = wfResult.ChifaNumFact
                } : null,
                ErrorMessage = wfResult?.ErrorMessage,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PrepareInvoice failed for {NumFact}", request.NumFact);
            return new FacadeResult<ChifaInvoiceResult>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaWorkflowResult>> ValidateInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _workflow.ValidateOnlyAsync(request, ct).ConfigureAwait(false);

            await _audit.LogOperationAsync("ValidateInvoice", "ChifaFacture",
                request.NumFact, result?.IsSuccess ?? false, sw.ElapsedMilliseconds,
                result?.ErrorMessage, _correlation.Current, ct).ConfigureAwait(false);

            return new FacadeResult<ChifaWorkflowResult>
            {
                Status = result?.IsSuccess == true ? FacadeOperationStatus.Success : FacadeOperationStatus.ValidationFailed,
                Data = result,
                ErrorMessage = result?.ErrorMessage,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateInvoice failed for {NumFact}", request.NumFact);
            return new FacadeResult<ChifaWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaInvoiceResult>> CreateInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var mode = await _modeProvider.GetModeAsync().ConfigureAwait(false);
            if (mode == ChifaIntegrationMode.ReadOnly)
                return new FacadeResult<ChifaInvoiceResult>
                {
                    Status = FacadeOperationStatus.NotAvailable,
                    ErrorMessage = "Cannot create invoice in ReadOnly mode",
                    DurationMs = sw.ElapsedMilliseconds
                };

            var exists = await _invoice.InvoiceExistsInChifaAsync(request.NumFact, ct).ConfigureAwait(false);
            if (exists)
                return new FacadeResult<ChifaInvoiceResult>
                {
                    Status = FacadeOperationStatus.Conflict,
                    ErrorMessage = $"Invoice {request.NumFact} already exists",
                    DurationMs = sw.ElapsedMilliseconds
                };

            var result = await _invoice.CreateInvoiceAsync(request, ct).ConfigureAwait(false);

            _metrics.Record("CreateInvoice", result.Success, sw.ElapsedMilliseconds);

            await _audit.LogOperationAsync("CreateInvoice", "ChifaFacture",
                request.NumFact, result.Success, sw.ElapsedMilliseconds,
                result.ErrorMessage, _correlation.Current, ct).ConfigureAwait(false);

            return new FacadeResult<ChifaInvoiceResult>
            {
                Status = result.Success ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result,
                ErrorMessage = result.ErrorMessage,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateInvoice failed for {NumFact}", request.NumFact);
            _metrics.Record("CreateInvoice", false, sw.ElapsedMilliseconds);
            return new FacadeResult<ChifaInvoiceResult>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<bool>> DeleteDraftAsync(
        string numFact, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var exists = await _invoice.InvoiceExistsInChifaAsync(numFact, ct).ConfigureAwait(false);
            if (!exists)
                return new FacadeResult<bool>
                {
                    Status = FacadeOperationStatus.NotFound,
                    Data = false,
                    ErrorMessage = $"Invoice {numFact} not found",
                    DurationMs = sw.ElapsedMilliseconds
                };

            await _audit.LogOperationAsync("DeleteDraft", "ChifaFacture",
                numFact, true, sw.ElapsedMilliseconds, userId: _correlation.Current, ct).ConfigureAwait(false);

            return new FacadeResult<bool>
            {
                Status = FacadeOperationStatus.Success,
                Data = true,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDraft failed for {NumFact}", numFact);
            return new FacadeResult<bool>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaInvoiceStatusSnapshot>> LoadInvoiceAsync(
        string numFact, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var snapshot = await _statusSync.LoadInvoiceAsync(numFact, ct).ConfigureAwait(false);

            return new FacadeResult<ChifaInvoiceStatusSnapshot>
            {
                Status = snapshot != null ? FacadeOperationStatus.Success : FacadeOperationStatus.NotFound,
                Data = snapshot,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoadInvoice failed for {NumFact}", numFact);
            return new FacadeResult<ChifaInvoiceStatusSnapshot>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaInvoiceStatusSnapshot>> GetInvoiceStatusAsync(
        string numFact, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var exists = await _invoice.InvoiceExistsInChifaAsync(numFact, ct).ConfigureAwait(false);
            var snapshot = new ChifaInvoiceStatusSnapshot
            {
                NumFact = numFact,
                ExistsInChifa = exists,
                Timestamp = DateTime.UtcNow
            };

            if (exists)
                snapshot.Visibility = VisibilityStatus.VisibleInFacture;

            return new FacadeResult<ChifaInvoiceStatusSnapshot>
            {
                Status = FacadeOperationStatus.Success,
                Data = snapshot,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetInvoiceStatus failed for {NumFact}", numFact);
            return new FacadeResult<ChifaInvoiceStatusSnapshot>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaBordereauStatusSnapshot>> GetBordereauStatusAsync(
        string numBord, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _bordereauStatus.GetStatusAsync(numBord, ct).ConfigureAwait(false);

            if (!result.IsSuccess)
                return new FacadeResult<ChifaBordereauStatusSnapshot>
                {
                    Status = FacadeOperationStatus.NotFound,
                    ErrorMessage = result.ErrorMessage,
                    DurationMs = sw.ElapsedMilliseconds
                };

            var snapshot = new ChifaBordereauStatusSnapshot
            {
                NumBord = result.NumBord,
                Etat = result.State.ToString(),
                ExistsInChifa = true,
                InvoiceCount = result.InvoiceCount,
                Timestamp = DateTime.UtcNow
            };

            return new FacadeResult<ChifaBordereauStatusSnapshot>
            {
                Status = FacadeOperationStatus.Success,
                Data = snapshot,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBordereauStatus failed for {NumBord}", numBord);
            return new FacadeResult<ChifaBordereauStatusSnapshot>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaSyncSummary>> SynchronizeAsync(
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var invoiceResult = await _invoiceSync.SynchronizeAsync(ct).ConfigureAwait(false);
            var bordereauResult = await _bordereauSync.SynchronizeAsync(ct).ConfigureAwait(false);

            var summary = new ChifaSyncSummary
            {
                InvoicesFound = invoiceResult.InvoicesFound,
                InvoicesUpdated = invoiceResult.InvoicesUpdated,
                BordereauxFound = bordereauResult.BordereauxFound,
                BordereauxUpdated = bordereauResult.BordereauxUpdated,
                Errors = invoiceResult.Errors.Concat(bordereauResult.Errors).ToList(),
                Timestamp = DateTime.UtcNow,
                DurationMs = sw.ElapsedMilliseconds
            };

            await _audit.LogOperationAsync("Synchronize", null, null,
                $"Invoices:{summary.InvoicesFound}/{summary.InvoicesUpdated} Bordereaux:{summary.BordereauxFound}/{summary.BordereauxUpdated}",
                summary.Errors.Count == 0, sw.ElapsedMilliseconds,
                summary.Errors.Count > 0 ? string.Join("; ", summary.Errors) : null,
                _correlation.Current, ct).ConfigureAwait(false);

            return new FacadeResult<ChifaSyncSummary>
            {
                Status = summary.Errors.Count == 0 ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = summary,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronize failed");
            return new FacadeResult<ChifaSyncSummary>
            {
                Status = FacadeOperationStatus.WriteFailed,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<ChifaStatusSnapshot>> RefreshStatusAsync(
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var snapshot = await _statusEngine.EvaluateAsync(ct).ConfigureAwait(false);
            return new FacadeResult<ChifaStatusSnapshot>
            {
                Status = FacadeOperationStatus.Success,
                Data = snapshot,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshStatus failed");
            return new FacadeResult<ChifaStatusSnapshot>
            {
                Status = FacadeOperationStatus.NotAvailable,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<DashboardOverview>> GetDashboardOverviewAsync(
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var status = await _statusEngine.EvaluateAsync(ct).ConfigureAwait(false);
            var lastSync = await _monitoring.GetLastSyncResultAsync(ct).ConfigureAwait(false);

            var overview = new DashboardOverview
            {
                Status = status,
                LastSync = lastSync,
                LastOperation = _monitoring.LastOperation,
                Timestamp = DateTime.UtcNow
            };

            return new FacadeResult<DashboardOverview>
            {
                Status = FacadeOperationStatus.Success,
                Data = overview,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDashboardOverview failed");
            return new FacadeResult<DashboardOverview>
            {
                Status = FacadeOperationStatus.NotAvailable,
                ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            return await _integration.IsChifaAvailableAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    public async Task<ChifaHealthStatus> GetHealthStatusAsync(CancellationToken ct = default)
    {
        try
        {
            return await _integration.GetHealthStatusAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetHealthStatus failed");
            return new ChifaHealthStatus
            {
                IsOnline = false, IsDatabaseConnected = false,
                IsTokenPresent = false, ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<(bool isPresent, string label, bool isValid)> GetTokenStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var present = await _token.IsTokenPresentAsync(ct).ConfigureAwait(false);
            if (!present) return (false, "", false);
            var info = await _token.GetTokenInfoAsync(ct).ConfigureAwait(false);
            return (true, info?.Label ?? "", info?.IsValid ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetTokenStatus failed");
            return (false, "", false);
        }
    }

    public async Task<ChifaSigningStatus> GetSigningStatusAsync(CancellationToken ct = default)
    {
        try
        {
            return await _signing.GetSigningStatusAsync("_global", ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetSigningStatus failed");
            return ChifaSigningStatus.NotSigned;
        }
    }

    public async Task<ChifaIntegrationMode> GetModeAsync(CancellationToken ct = default)
    {
        return await _modeProvider.GetModeAsync().ConfigureAwait(false);
    }

    public async Task<FacadeResult<ChifaWorkflowResult>> ExecuteFullWorkflowAsync(
        ChifaInvoiceRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _workflow.ExecuteFullWorkflowAsync(request, _correlation.Current ?? "system", ct).ConfigureAwait(false);
            return new FacadeResult<ChifaWorkflowResult>
            {
                Status = result.IsSuccess ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result,
                ErrorMessage = result.ErrorMessage,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExecuteFullWorkflow failed for {NumFact}", request.NumFact);
            return new FacadeResult<ChifaWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed, ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public Task<List<ChifaWorkflowAuditEntry>> GetWorkflowAuditLogAsync(
        int maxEntries = 20, CancellationToken ct = default)
    {
        try
        {
            var entries = _workflow.GetAuditLog().Take(maxEntries).ToList();
            return Task.FromResult(entries);
        }
        catch
        {
            return Task.FromResult(new List<ChifaWorkflowAuditEntry>());
        }
    }

    public Task<List<BordereauWorkflowSummary>> GetAllBordereauxAsync(CancellationToken ct = default)
    {
        try
        {
            return Task.FromResult(_bordereauStatus.GetAllBordereaux().ToList());
        }
        catch
        {
            return Task.FromResult(new List<BordereauWorkflowSummary>());
        }
    }

    public Task<List<BordereauWorkflowAuditEntry>> GetBordereauAuditLogAsync(
        string? numBord = null, CancellationToken ct = default)
    {
        try
        {
            return Task.FromResult(_bordereauStatus.GetAuditLog(numBord).ToList());
        }
        catch
        {
            return Task.FromResult(new List<BordereauWorkflowAuditEntry>());
        }
    }

    public async Task<FacadeResult<BordereauWorkflowResult>> CreateBordereauAsync(
        string numBord, string codeCentre, List<string> invoiceNumbers, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _bordereauStatus.CreateBordereauAsync(numBord, codeCentre, invoiceNumbers, _correlation.Current ?? "system", ct).ConfigureAwait(false);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = result.IsSuccess ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result,
                ErrorMessage = result.ErrorMessage,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateBordereau failed for {NumBord}", numBord);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed, ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<BordereauWorkflowResult>> ValidateBordereauAsync(
        string numBord, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _bordereauStatus.ValidateBordereauAsync(numBord, ct).ConfigureAwait(false);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = result.IsSuccess ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result, ErrorMessage = result.ErrorMessage, DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateBordereau failed for {NumBord}", numBord);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed, ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<BordereauWorkflowResult>> SignBordereauAsync(
        string numBord, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _bordereauStatus.SignBordereauAsync(numBord, _correlation.Current ?? "system", ct).ConfigureAwait(false);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = result.IsSuccess ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result, ErrorMessage = result.ErrorMessage, DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignBordereau failed for {NumBord}", numBord);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed, ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<BordereauWorkflowResult>> CloseBordereauAsync(
        string numBord, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _bordereauStatus.CloseBordereauAsync(numBord, _correlation.Current ?? "system", ct).ConfigureAwait(false);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = result.IsSuccess ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result, ErrorMessage = result.ErrorMessage, DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CloseBordereau failed for {NumBord}", numBord);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed, ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task<FacadeResult<BordereauWorkflowResult>> TransmitBordereauAsync(
        string numBord, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _correlation.GetOrCreate();
            var result = await _bordereauStatus.TransmitBordereauAsync(numBord, _correlation.Current ?? "system", ct).ConfigureAwait(false);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = result.IsSuccess ? FacadeOperationStatus.Success : FacadeOperationStatus.WriteFailed,
                Data = result, ErrorMessage = result.ErrorMessage, DurationMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TransmitBordereau failed for {NumBord}", numBord);
            return new FacadeResult<BordereauWorkflowResult>
            {
                Status = FacadeOperationStatus.WriteFailed, ErrorMessage = ex.Message,
                DurationMs = sw.ElapsedMilliseconds
            };
        }
    }

    public void Dispose()
    {
        _monitoring.Dispose();
    }
}
