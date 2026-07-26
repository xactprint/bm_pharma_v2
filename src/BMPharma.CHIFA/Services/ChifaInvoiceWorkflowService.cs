using System.Collections.Concurrent;
using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public interface IChifaInvoiceWorkflowService
{
    Task<ChifaWorkflowResult> ExecuteFullWorkflowAsync(ChifaInvoiceRequest request, string userId = "system", CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> ValidateOnlyAsync(ChifaInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> PrepareInvoiceAsync(ChifaInvoiceRequest request, string userId = "system", CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> CreateInDatabaseAsync(string numFact, string userId = "system", CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> CheckVisibilityAsync(string numFact, CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> SignBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> CloseBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default);
    Task<ChifaWorkflowResult> AssignBordereauAsync(string numFact, string userId = "system", CancellationToken cancellationToken = default);
    IReadOnlyList<ChifaWorkflowAuditEntry> GetAuditLog();
    void ClearAuditLog();
}

public class ChifaInvoiceWorkflowService : IChifaInvoiceWorkflowService
{
    private readonly IChifaIntegrationService _integrationService;
    private readonly IChifaInvoiceService _invoiceService;
    private readonly IChifaBordereauService _bordereauService;
    private readonly IChifaSigningService _signingService;
    private readonly IChifaTokenService _tokenService;
    private readonly IChifaAuditService _auditService;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly ChifaInvoiceValidator _invoiceValidator;
    private readonly ChifaBordereauValidator _bordereauValidator;
    private readonly ChifaInvoiceMapper _invoiceMapper;
    private readonly ChifaWorkflowStateMachine _stateMachine;
    private readonly ILogger<ChifaInvoiceWorkflowService> _logger;

    private readonly ConcurrentBag<ChifaWorkflowAuditEntry> _auditLog = new();
    private readonly ConcurrentDictionary<string, ChifaWorkflowState> _invoiceStates = new();

    public ChifaInvoiceWorkflowService(
        IChifaIntegrationService integrationService,
        IChifaInvoiceService invoiceService,
        IChifaBordereauService bordereauService,
        IChifaSigningService signingService,
        IChifaTokenService tokenService,
        IChifaAuditService auditService,
        ChifaIntegrationModeProvider modeProvider,
        ChifaInvoiceValidator invoiceValidator,
        ChifaBordereauValidator bordereauValidator,
        ChifaInvoiceMapper invoiceMapper,
        ChifaWorkflowStateMachine stateMachine,
        ILogger<ChifaInvoiceWorkflowService> logger)
    {
        _integrationService = integrationService;
        _invoiceService = invoiceService;
        _bordereauService = bordereauService;
        _signingService = signingService;
        _tokenService = tokenService;
        _auditService = auditService;
        _modeProvider = modeProvider;
        _invoiceValidator = invoiceValidator;
        _bordereauValidator = bordereauValidator;
        _invoiceMapper = invoiceMapper;
        _stateMachine = stateMachine;
        _logger = logger;
    }

    public async Task<ChifaWorkflowResult> ExecuteFullWorkflowAsync(ChifaInvoiceRequest request, string userId = "system", CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var mode = _modeProvider.CurrentMode;
        var isReadOnly = _modeProvider.IsReadOnly;

        _logger.LogInformation("[WORKFLOW-{CorrelationId}] Starting full workflow for {NumFact} (Mode: {Mode})",
            correlationId, request.NumFact, mode);

        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumFact = request.NumFact,
            Mode = mode,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Step 1: Validate
            await AuditStepAsync(correlationId, "VALIDATE", "facture", request.NumFact, userId, "BM_PHARMA", "BM_VALIDATION");
            var validationResult = await ValidateOnlyAsync(request, cancellationToken);
            result.ValidationErrors.AddRange(validationResult.ValidationErrors);

            if (!validationResult.IsSuccess)
            {
                result.Step = "VALIDATION_FAILED";
                result.IsSuccess = false;
                result.ErrorMessage = $"Validation failed with {result.ValidationErrors.Count} error(s)";
                await AuditStepAsync(correlationId, "VALIDATE", "facture", request.NumFact, userId,
                    "BM_VALIDATION", "BM_VALIDATION", false, result.ErrorMessage);
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.State = ChifaWorkflowState.Validated;
            _invoiceStates[request.NumFact] = ChifaWorkflowState.Validated;
            result.Step = "VALIDATED";

            if (isReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Cette opération est simulée. Aucune donnée CHIFA n'est modifiée.";
                result.State = ChifaWorkflowState.PreparedForChifa;
                result.IsSuccess = true;
                await AuditStepAsync(correlationId, "READONLY_SIMULATION", "facture", request.NumFact, userId,
                    "BM_PHARMA", "CHIFA_SIMULATION", true, "ReadOnly simulation completed");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            // Step 2: Prepare for CHIFA
            _invoiceValidator.ApplyDefaults(request);
            foreach (var line in request.Lines)
                _invoiceValidator.ApplyLineDefaults(line);

            result.State = ChifaWorkflowState.PreparedForChifa;
            _invoiceStates[request.NumFact] = ChifaWorkflowState.PreparedForChifa;
            result.Step = "PREPARED";
            await AuditStepAsync(correlationId, "PREPARE", "facture", request.NumFact, userId, "BM_PHARMA", "BM_VALIDATION");

            // Step 3: Write to CHIFA database
            await AuditStepAsync(correlationId, "WRITE_TO_CHIFA", "facture", request.NumFact, userId, "BM_PHARMA", "CHIFA_POSTGRESQL");
            var invoiceResult = await _invoiceService.CreateInvoiceAsync(request, cancellationToken);

            if (!invoiceResult.Success)
            {
                result.State = ChifaWorkflowState.Failed;
                _invoiceStates[request.NumFact] = ChifaWorkflowState.Failed;
                result.Step = "WRITE_FAILED";
                result.IsSuccess = false;
                result.ErrorMessage = invoiceResult.ErrorMessage;
                await AuditStepAsync(correlationId, "WRITE_TO_CHIFA", "facture", request.NumFact, userId,
                    "BM_PHARMA", "CHIFA_POSTGRESQL", false, result.ErrorMessage);
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.State = ChifaWorkflowState.WrittenToChifa;
            _invoiceStates[request.NumFact] = ChifaWorkflowState.WrittenToChifa;
            result.Step = "WRITTEN_TO_CHIFA";
            result.ChifaNumFact = invoiceResult.ChifaNumFact;

            // Step 4: Check visibility
            await AuditStepAsync(correlationId, "CHECK_VISIBILITY", "facture", request.NumFact, userId, "CHIFA_POSTGRESQL", "BM_PHARMA");
            var isVisible = await _invoiceService.InvoiceExistsInChifaAsync(request.NumFact, cancellationToken);

            if (!isVisible)
            {
                result.State = ChifaWorkflowState.Failed;
                _invoiceStates[request.NumFact] = ChifaWorkflowState.Failed;
                result.Step = "VISIBILITY_CHECK_FAILED";
                result.IsSuccess = false;
                result.ErrorMessage = "Invoice not visible in CHIFA after write. Possible CHIFA replication delay.";
                await AuditStepAsync(correlationId, "CHECK_VISIBILITY", "facture", request.NumFact, userId,
                    "CHIFA_POSTGRESQL", "BM_PHARMA", false, result.ErrorMessage);
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.State = ChifaWorkflowState.VisibleInChifa;
            _invoiceStates[request.NumFact] = ChifaWorkflowState.VisibleInChifa;
            result.Step = "VISIBLE_IN_CHIFA";
            result.RequiresAction = true;
            result.ActionDescription = "Signature réglementaire obligatoire. Ouvrez CHIFA-OFFICINE et effectuez la signature avec le token professionnel.";

            await AuditStepAsync(correlationId, "VISIBLE_IN_CHIFA", "facture", request.NumFact, userId,
                "CHIFA_POSTGRESQL", "BM_PHARMA", true, "Invoice visible in CHIFA. Awaiting signature.");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WORKFLOW-{CorrelationId}] Unexpected error for {NumFact}", correlationId, request.NumFact);
            result.State = ChifaWorkflowState.Failed;
            result.Step = "UNEXPECTED_ERROR";
            result.IsSuccess = false;
            result.ErrorMessage = $"Unexpected error: {ex.Message}";
            await AuditStepAsync(correlationId, "ERROR", "facture", request.NumFact, userId,
                "BM_PHARMA", "BM_PHARMA", false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            return result;
        }
    }

    public async Task<ChifaWorkflowResult> ValidateOnlyAsync(ChifaInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumFact = request.NumFact,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var health = await _integrationService.GetHealthStatusAsync(cancellationToken);
            if (!health.IsOnline)
            {
                result.ValidationErrors.Add(new ChifaWorkflowValidationError
                {
                    Code = "CHIFA_OFFLINE",
                    Field = "connection",
                    Message = "CHIFA database is not available. Check PostgreSQL connection."
                });
            }

            var validationResult = _invoiceValidator.Validate(request);
            foreach (var error in validationResult.Errors)
            {
                result.ValidationErrors.Add(new ChifaWorkflowValidationError
                {
                    Code = error.Code,
                    Field = error.Field,
                    Message = error.Message
                });
            }

            if (request.Lines.Count > 0)
            {
                var montFact = request.Lines.Sum(l => l.Quantite * l.PrixUnit);
                if (montFact <= 0)
                {
                    result.ValidationErrors.Add(new ChifaWorkflowValidationError
                    {
                        Code = "MONT_FACT_INVALID",
                        Field = "mont_fact",
                        Message = "Total invoice amount must be positive"
                    });
                }

                foreach (var line in request.Lines)
                {
                    if (line.Quantite <= 0)
                    {
                        result.ValidationErrors.Add(new ChifaWorkflowValidationError
                        {
                            Code = "QTE_INVALID",
                            Field = $"line.{line.NumEnr}.qte",
                            Message = $"Quantity for line {line.NumEnr} must be positive"
                        });
                    }

                    if (line.PrixUnit <= 0)
                    {
                        result.ValidationErrors.Add(new ChifaWorkflowValidationError
                        {
                            Code = "PPA_INVALID",
                            Field = $"line.{line.NumEnr}.ppa",
                            Message = $"PPA price for line {line.NumEnr} must be positive"
                        });
                    }
                }
            }

            result.IsSuccess = result.ValidationErrors.Count == 0;
            result.Step = result.IsSuccess ? "VALIDATION_PASSED" : "VALIDATION_FAILED";

            if (result.IsSuccess)
            {
                var montFact = request.Lines.Sum(l => l.Quantite * l.PrixUnit);
                result.ComputedMontFact = Math.Round(montFact, 2);
                result.ComputedMontAs = Math.Round(montFact * 0.70m, 2);
                result.ComputedMontMut = Math.Round(montFact - result.ComputedMontAs, 2);
            }
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = $"Validation error: {ex.Message}";
            result.ValidationErrors.Add(new ChifaWorkflowValidationError
            {
                Code = "VALIDATION_EXCEPTION",
                Field = "system",
                Message = ex.Message
            });
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public Task<ChifaWorkflowResult> PrepareInvoiceAsync(ChifaInvoiceRequest request, string userId = "system", CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumFact = request.NumFact,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            _invoiceValidator.ApplyDefaults(request);
            foreach (var line in request.Lines)
                _invoiceValidator.ApplyLineDefaults(line);

            var montFact = request.Lines.Sum(l => l.Quantite * l.PrixUnit);
            result.ComputedMontFact = Math.Round(montFact, 2);
            result.ComputedMontAs = Math.Round(montFact * 0.70m, 2);
            result.ComputedMontMut = Math.Round(montFact - result.ComputedMontAs, 2);

            result.State = ChifaWorkflowState.PreparedForChifa;
            result.Step = "PREPARED";
            result.IsSuccess = true;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Step = "PREPARE_FAILED";
            result.ErrorMessage = ex.Message;
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return Task.FromResult(result);
    }

    public async Task<ChifaWorkflowResult> CreateInDatabaseAsync(string numFact, string userId = "system", CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumFact = numFact,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var exists = await _invoiceService.InvoiceExistsInChifaAsync(numFact, cancellationToken);
            if (exists)
            {
                result.IsSuccess = false;
                result.Step = "ALREADY_EXISTS";
                result.ErrorMessage = $"Invoice {numFact} already exists in CHIFA";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.Step = "CREATED_IN_DATABASE";
            result.State = ChifaWorkflowState.WrittenToChifa;
            result.IsSuccess = true;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Step = "CREATE_FAILED";
            result.ErrorMessage = ex.Message;
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public async Task<ChifaWorkflowResult> CheckVisibilityAsync(string numFact, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumFact = numFact,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var isVisible = await _invoiceService.InvoiceExistsInChifaAsync(numFact, cancellationToken);
            result.IsSuccess = isVisible;
            result.Step = isVisible ? "VISIBLE" : "NOT_VISIBLE";
            result.State = isVisible ? ChifaWorkflowState.VisibleInChifa : ChifaWorkflowState.WrittenToChifa;

            if (!isVisible)
            {
                result.ErrorMessage = "Invoice not yet visible in CHIFA. Possible replication delay.";
                result.RequiresAction = false;
            }
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Step = "VISIBILITY_CHECK_FAILED";
            result.ErrorMessage = ex.Message;
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public async Task<ChifaWorkflowResult> SignBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumBord = numBord,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var tokenPresent = await _tokenService.IsTokenPresentAsync(cancellationToken);
            if (!tokenPresent)
            {
                result.IsSuccess = false;
                result.Step = "TOKEN_NOT_PRESENT";
                result.ErrorMessage = "Professional token not present. Signing requires CHIFA-OFFICINE with the physical token.";
                result.RequiresAction = true;
                result.ActionDescription = "Insérez le token professionnel puis ouvrez CHIFA-OFFICINE pour signer le bordereau.";
                result.ActionApplication = "CHIFA-OFFICINE";
                result.ActionBmPharmaWaits = "BM Pharma attend que la signature soit effectuée.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            var signResult = await _bordereauService.SignBordereauAsync(numBord, cancellationToken);
            result.IsSuccess = signResult.Success;
            result.Step = signResult.Success ? "SIGNED" : "SIGN_FAILED";
            result.State = signResult.Success ? ChifaWorkflowState.Signed : ChifaWorkflowState.Failed;
            result.SignatureId = signResult.SignatureId;
            result.ErrorMessage = signResult.ErrorMessage;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Step = "SIGN_ERROR";
            result.ErrorMessage = ex.Message;
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public async Task<ChifaWorkflowResult> CloseBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumBord = numBord,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var closeResult = await _bordereauService.CloseBordereauAsync(numBord, cancellationToken);
            result.IsSuccess = closeResult.Success;
            result.Step = closeResult.Success ? "CLOSED" : "CLOSE_FAILED";
            result.State = closeResult.Success ? ChifaWorkflowState.BordereauClosed : ChifaWorkflowState.Failed;
            result.ErrorMessage = closeResult.ErrorMessage;

            if (!closeResult.Success)
            {
                result.RequiresAction = true;
                result.ActionDescription = "Le bordereau doit être signé avant la clôture.";
                result.ActionApplication = "CHIFA-OFFICINE";
            }
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Step = "CLOSE_ERROR";
            result.ErrorMessage = ex.Message;
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public async Task<ChifaWorkflowResult> AssignBordereauAsync(string numFact, string userId = "system", CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ChifaWorkflowResult
        {
            CorrelationId = correlationId,
            NumFact = numFact,
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var nextNumBord = await _bordereauService.GetNextBordereauNumberAsync(cancellationToken);
            result.NumBord = nextNumBord;
            result.State = ChifaWorkflowState.BordereauAssigned;
            result.Step = "BORDEREAU_ASSIGNED";
            result.IsSuccess = true;
            result.RequiresAction = true;
            result.ActionDescription = "Bordereau préparé. Signature puis clôture nécessaires via CHIFA-OFFICINE.";
            result.ActionApplication = "CHIFA-OFFICINE";
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Step = "ASSIGN_FAILED";
            result.ErrorMessage = ex.Message;
        }

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public IReadOnlyList<ChifaWorkflowAuditEntry> GetAuditLog()
    {
        return _auditLog.OrderByDescending(e => e.Timestamp).ToList().AsReadOnly();
    }

    public void ClearAuditLog()
    {
        while (_auditLog.TryTake(out _)) { }
    }

    private async Task AuditStepAsync(
        string correlationId, string operation, string? entityType, string? entityKey,
        string userId, string source, string destination,
        bool success = true, string? details = null)
    {
        var entry = new ChifaWorkflowAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId,
            Operation = operation,
            EntityType = entityType,
            EntityKey = entityKey,
            UserId = userId,
            Source = source,
            Destination = destination,
            Result = success ? "SUCCESS" : "ERROR",
            Details = details
        };

        _auditLog.Add(entry);

        try
        {
            await _auditService.LogOperationAsync(
                operation, entityType, entityKey,
                $"[{correlationId}] {operation} from {source} to {destination}",
                success, 0, details, userId);
        }
        catch
        {
            _logger.LogWarning("Failed to persist audit entry for {Operation} on {EntityKey}", operation, entityKey);
        }
    }
}

public class ChifaWorkflowResult
{
    public string CorrelationId { get; set; } = string.Empty;
    public string NumFact { get; set; } = string.Empty;
    public string? NumBord { get; set; }
    public string? ChifaNumFact { get; set; }
    public string? SignatureId { get; set; }
    public ChifaIntegrationMode Mode { get; set; }
    public ChifaWorkflowState State { get; set; } = ChifaWorkflowState.Draft;
    public string Step { get; set; } = "PENDING";
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SimulationMessage { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionDescription { get; set; }
    public string? ActionApplication { get; set; }
    public string? ActionBmPharmaWaits { get; set; }
    public decimal ComputedMontFact { get; set; }
    public decimal ComputedMontAs { get; set; }
    public decimal ComputedMontMut { get; set; }
    public List<ChifaWorkflowValidationError> ValidationErrors { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public long DurationMs { get; set; }
}

public class ChifaWorkflowValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ChifaWorkflowAuditEntry
{
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityKey { get; set; }
    public string? UserId { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Details { get; set; }
}
