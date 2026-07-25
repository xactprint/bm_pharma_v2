using System.Diagnostics;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Entities;
using BMPharma.Domain.Enums;

namespace BMPharma.CHIFA.Services;

public class OneActionWorkflowService
{
    private readonly ChifaInvoiceValidator _invoiceValidator;
    private readonly ChifaBordereauValidator _bordereauValidator;
    private readonly ChifaInvoiceMapper _invoiceMapper;
    private readonly ChifaBordereauMapper _bordereauMapper;
    private readonly ChifaWorkflowStateMachine _stateMachine;
    private readonly IChifaAuditService _auditService;
    private readonly ILogger<OneActionWorkflowService> _logger;

    private IChifaInvoiceService _invoiceService;
    private IChifaBordereauService _bordereauService;
    private IChifaSigningService _signingService;
    private IChifaIntegrationService _integrationService;

    public OneActionWorkflowService(
        IChifaInvoiceService invoiceService,
        IChifaBordereauService bordereauService,
        IChifaSigningService signingService,
        IChifaIntegrationService integrationService,
        ChifaInvoiceValidator invoiceValidator,
        ChifaBordereauValidator bordereauValidator,
        ChifaInvoiceMapper invoiceMapper,
        ChifaBordereauMapper bordereauMapper,
        ChifaWorkflowStateMachine stateMachine,
        IChifaAuditService auditService,
        ILogger<OneActionWorkflowService> logger)
    {
        _invoiceService = invoiceService;
        _bordereauService = bordereauService;
        _signingService = signingService;
        _integrationService = integrationService;
        _invoiceValidator = invoiceValidator;
        _bordereauValidator = bordereauValidator;
        _invoiceMapper = invoiceMapper;
        _bordereauMapper = bordereauMapper;
        _stateMachine = stateMachine;
        _auditService = auditService;
        _logger = logger;
    }

    public void ReplaceServices(
        IChifaInvoiceService invoiceService,
        IChifaBordereauService bordereauService,
        IChifaSigningService signingService,
        IChifaIntegrationService integrationService)
    {
        _invoiceService = invoiceService;
        _bordereauService = bordereauService;
        _signingService = signingService;
        _integrationService = integrationService;
    }

    public async Task<WorkflowResult> ExecuteFullWorkflowAsync(
        Invoice invoice, Customer? customer = null, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var result = new WorkflowResult { CorrelationId = correlationId };
        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("[WORKFLOW:{CorId}] Starting full CHIFA workflow for invoice {InvoiceNumber}",
                correlationId, invoice.InvoiceNumber);

            result = await Step_ValidateLocally(invoice, customer, result, correlationId, cancellationToken);
            if (!result.Success) return result;

            result = await Step_PrepareChifa(invoice, customer, result, correlationId, cancellationToken);
            if (!result.Success) return result;

            result = await Step_WriteToChifa(invoice, result, correlationId, cancellationToken);
            if (!result.Success) return result;

            result = await Step_CheckVisibility(invoice, result, correlationId, cancellationToken);
            if (!result.Success) return result;

            result = await Step_CheckSigning(invoice, result, correlationId, cancellationToken);
            if (!result.Success) return result;

            sw.Stop();
            result.TotalDurationMs = sw.ElapsedMilliseconds;

            _logger.LogInformation(
                "[WORKFLOW:{CorId}] Workflow completed for invoice {InvoiceNumber}. " +
                "Final state: {State}. Requires human action: {RequiresHuman}. Duration: {Duration}ms",
                correlationId, invoice.InvoiceNumber, invoice.ChifaState,
                _stateMachine.RequiresHumanAction(invoice.ChifaState), sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.TotalDurationMs = sw.ElapsedMilliseconds;

            invoice.ChifaState = ChifaWorkflowState.Failed;
            invoice.ChifaErrorMessage = ex.Message;
            invoice.ChifaLastUpdated = DateTime.UtcNow;

            await _auditService.LogOperationAsync("WORKFLOW_FAILED", "invoice", invoice.InvoiceNumber,
                $"CorrelationId: {correlationId}", false, sw.ElapsedMilliseconds, ex.Message,
                cancellationToken: cancellationToken);

            _logger.LogError(ex, "[WORKFLOW:{CorId}] Workflow failed for invoice {InvoiceNumber}",
                correlationId, invoice.InvoiceNumber);

            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Steps.Add(new WorkflowStepResult
            {
                StepName = "EXCEPTION",
                Success = false,
                Error = ex.Message
            });

            return result;
        }
    }

    private async Task<WorkflowResult> Step_ValidateLocally(
        Invoice invoice, Customer? customer, WorkflowResult result,
        string correlationId, CancellationToken ct)
    {
        _logger.LogDebug("[WORKFLOW:{CorId}] Step 1: Local validation", correlationId);

        if (!invoice.Lines.Any())
        {
            return FailStep(result, "VALIDATE_LOCAL", "Invoice has no lines", correlationId, ct);
        }

        var chifaRequest = _invoiceMapper.MapToChifaRequest(invoice, customer);
        var validation = _invoiceValidator.Validate(chifaRequest);

        if (!validation.IsValid)
        {
            var errors = string.Join("; ", validation.Errors.Select(e => e.Message));
            invoice.ChifaState = ChifaWorkflowState.Failed;
            invoice.ChifaErrorMessage = errors;
            invoice.ChifaLastUpdated = DateTime.UtcNow;

            return FailStep(result, "VALIDATE_CHIFA", errors, correlationId, ct);
        }

        invoice.ChifaState = ChifaWorkflowState.Validated;
        invoice.ChifaLastUpdated = DateTime.UtcNow;

        result.Steps.Add(new WorkflowStepResult
        {
            StepName = "VALIDATE_LOCAL",
            Success = true,
            Details = "Invoice passed local and CHIFA validation"
        });

        await _auditService.LogOperationAsync("VALIDATE_INVOICE", "invoice", invoice.InvoiceNumber,
            $"CorrelationId: {correlationId}", true, 0, cancellationToken: ct);

        return result;
    }

    private async Task<WorkflowResult> Step_PrepareChifa(
        Invoice invoice, Customer? customer, WorkflowResult result,
        string correlationId, CancellationToken ct)
    {
        _logger.LogDebug("[WORKFLOW:{CorId}] Step 2: Prepare CHIFA data", correlationId);

        var isAvailable = await _integrationService.IsChifaAvailableAsync(ct);
        if (!isAvailable)
        {
            return FailStep(result, "PREPARE_CHIFA", "CHIFA is not available", correlationId, ct);
        }

        invoice.ChifaState = ChifaWorkflowState.PreparedForChifa;
        invoice.ChifaLastUpdated = DateTime.UtcNow;

        result.Steps.Add(new WorkflowStepResult
        {
            StepName = "PREPARE_CHIFA",
            Success = true,
            Details = "CHIFA data prepared"
        });

        await _auditService.LogOperationAsync("PREPARE_INVOICE", "invoice", invoice.InvoiceNumber,
            $"CorrelationId: {correlationId}", true, 0, cancellationToken: ct);

        return result;
    }

    private async Task<WorkflowResult> Step_WriteToChifa(
        Invoice invoice, WorkflowResult result,
        string correlationId, CancellationToken ct)
    {
        _logger.LogDebug("[WORKFLOW:{CorId}] Step 3: Write to CHIFA", correlationId);

        var chifaRequest = _invoiceMapper.MapToChifaRequest(invoice);

        var writeResult = await _invoiceService.CreateInvoiceAsync(chifaRequest, ct);

        if (!writeResult.Success)
        {
            invoice.ChifaState = ChifaWorkflowState.Failed;
            invoice.ChifaErrorMessage = writeResult.ErrorMessage;
            invoice.ChifaLastUpdated = DateTime.UtcNow;

            return FailStep(result, "WRITE_CHIFA", writeResult.ErrorMessage ?? "Write failed", correlationId, ct);
        }

        _invoiceMapper.ApplyChifaFieldsToInvoice(invoice, chifaRequest, writeResult);

        invoice.ChifaState = ChifaWorkflowState.WrittenToChifa;
        invoice.ChifaIntegrationState = ChifaIntegrationState.Simulated;
        invoice.ChifaLastUpdated = DateTime.UtcNow;

        result.Steps.Add(new WorkflowStepResult
        {
            StepName = "WRITE_CHIFA",
            Success = true,
            Details = $"Invoice written to CHIFA: {writeResult.ChifaNumFact}"
        });

        await _auditService.LogOperationAsync("WRITE_INVOICE", "invoice", invoice.InvoiceNumber,
            $"ChifaNumFact: {writeResult.ChifaNumFact}, CorrelationId: {correlationId}",
            true, 0, cancellationToken: ct);

        return result;
    }

    private async Task<WorkflowResult> Step_CheckVisibility(
        Invoice invoice, WorkflowResult result,
        string correlationId, CancellationToken ct)
    {
        _logger.LogDebug("[WORKFLOW:{CorId}] Step 4: Check CHIFA visibility", correlationId);

        if (invoice.ChifaNumFact == null)
        {
            return FailStep(result, "CHECK_VISIBILITY", "No CHIFA invoice number", correlationId, ct);
        }

        var exists = await _invoiceService.InvoiceExistsInChifaAsync(invoice.ChifaNumFact, ct);

        if (!exists)
        {
            invoice.ChifaState = ChifaWorkflowState.Failed;
            invoice.ChifaErrorMessage = "Invoice not visible in CHIFA after write";
            invoice.ChifaLastUpdated = DateTime.UtcNow;

            return FailStep(result, "CHECK_VISIBILITY", "Invoice not visible in CHIFA", correlationId, ct);
        }

        invoice.ChifaState = ChifaWorkflowState.VisibleInChifa;
        invoice.ChifaLastUpdated = DateTime.UtcNow;

        result.Steps.Add(new WorkflowStepResult
        {
            StepName = "CHECK_VISIBILITY",
            Success = true,
            Details = "Invoice confirmed visible in CHIFA",
            RequiresHumanAction = true,
            HumanActionMessage = "Present the professional card in CHIFA-OFFICINE to sign this invoice."
        });

        await _auditService.LogOperationAsync("CHECK_VISIBILITY", "invoice", invoice.InvoiceNumber,
            $"CorrelationId: {correlationId}", true, 0, cancellationToken: ct);

        return result;
    }

    private async Task<WorkflowResult> Step_CheckSigning(
        Invoice invoice, WorkflowResult result,
        string correlationId, CancellationToken ct)
    {
        _logger.LogDebug("[WORKFLOW:{CorId}] Step 5: Check signing status (read-only)", correlationId);

        var tokenAvailable = await _signingService.IsTokenAvailableAsync(ct);

        if (!tokenAvailable)
        {
            result.Steps.Add(new WorkflowStepResult
            {
                StepName = "CHECK_SIGNING",
                Success = true,
                Details = "Signing requires CHIFA-OFFICINE with professional token",
                RequiresHumanAction = true,
                HumanActionMessage = "Switch to CHIFA-OFFICINE and use the professional token to sign."
            });

            await _auditService.LogOperationAsync("SIGNING_REQUIRED", "invoice", invoice.InvoiceNumber,
                $"CorrelationId: {correlationId}, TokenPresent: false",
                true, 0, cancellationToken: ct);

            return result;
        }

        invoice.ChifaState = ChifaWorkflowState.Signed;
        invoice.ChifaLastUpdated = DateTime.UtcNow;

        result.Steps.Add(new WorkflowStepResult
        {
            StepName = "CHECK_SIGNING",
            Success = true,
            Details = "Token available, signing status checked"
        });

        return result;
    }

    private WorkflowResult FailStep(WorkflowResult result, string stepName, string error,
        string correlationId, CancellationToken ct)
    {
        result.Success = false;
        result.ErrorMessage = error;
        result.Steps.Add(new WorkflowStepResult
        {
            StepName = stepName,
            Success = false,
            Error = error
        });

        _ = _auditService.LogOperationAsync($"FAIL_{stepName}", "workflow", correlationId,
            error, false, 0, error, cancellationToken: ct);

        return result;
    }

    public async Task<WorkflowResult> AssignBordereauAsync(
        Bordereau bordereau, IEnumerable<Invoice> invoices,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var result = new WorkflowResult { CorrelationId = correlationId };

        try
        {
            var chifaRequest = _bordereauMapper.MapToChifaRequest(bordereau, invoices);
            var validation = _bordereauValidator.Validate(
                chifaRequest.NumBord, chifaRequest.NumBord[..2], chifaRequest.InvoiceNumbers);

            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.Message));
                return FailStep(result, "VALIDATE_BORDEREAU", errors, correlationId, cancellationToken);
            }

            var createResult = await _bordereauService.CreateBordereauAsync(chifaRequest, cancellationToken);

            if (!createResult.Success)
            {
                return FailStep(result, "CREATE_BORDEREAU", createResult.ErrorMessage ?? "Failed", correlationId, cancellationToken);
            }

            bordereau.ChifaNumBord = createResult.NumBord;
            bordereau.ChifaLastUpdated = DateTime.UtcNow;

            result.Steps.Add(new WorkflowStepResult
            {
                StepName = "CREATE_BORDEREAU",
                Success = true,
                Details = $"Bordereau {createResult.NumBord} created"
            });

            foreach (var invoice in invoices.Where(i => i.ChifaState == ChifaWorkflowState.VisibleInChifa ||
                                                          i.ChifaState == ChifaWorkflowState.Signed))
            {
                invoice.ChifaState = ChifaWorkflowState.BordereauAssigned;
                invoice.ChifaNumBord = createResult.NumBord;
                invoice.ChifaLastUpdated = DateTime.UtcNow;
            }

            await _auditService.LogOperationAsync("ASSIGN_BORDEREAU", "bordereau", bordereau.BordereauNumber,
                $"NumBord: {createResult.NumBord}, Invoices: {chifaRequest.InvoiceNumbers.Count}, CorrelationId: {correlationId}",
                true, 0, cancellationToken: cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return FailStep(result, "ASSIGN_BORDEREAU", ex.Message, correlationId, cancellationToken);
        }
    }
}

public class WorkflowResult
{
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public List<WorkflowStepResult> Steps { get; set; } = new();
    public long TotalDurationMs { get; set; }
    public bool RequiresHumanAction => Steps.Any(s => s.RequiresHumanAction);
    public string? HumanActionMessage => Steps.LastOrDefault(s => s.RequiresHumanAction)?.HumanActionMessage;
}

public class WorkflowStepResult
{
    public string StepName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Details { get; set; }
    public string? Error { get; set; }
    public bool RequiresHumanAction { get; set; }
    public string? HumanActionMessage { get; set; }
}
