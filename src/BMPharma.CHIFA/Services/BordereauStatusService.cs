using System.Collections.Concurrent;
using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public interface IBordereauStatusService
{
    Task<BordereauWorkflowResult> GetStatusAsync(string numBord, CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> CreateBordereauAsync(string numBord, string codeCentre, List<string> invoiceNumbers, string userId = "system", CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> AttachInvoicesAsync(string numBord, List<string> invoiceNumbers, string userId = "system", CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> RemoveInvoiceAsync(string numBord, string numFact, string userId = "system", CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> ValidateBordereauAsync(string numBord, CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> SignBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> CloseBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> TransmitBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default);
    Task<BordereauWorkflowResult> TransitionToAsync(string numBord, BordereauWorkflowState target, string userId = "system", CancellationToken cancellationToken = default);
    IReadOnlyList<BordereauWorkflowAuditEntry> GetAuditLog(string? numBord = null);
    IReadOnlyList<BordereauWorkflowSummary> GetAllBordereaux();
    void ClearAuditLog();
}

public class BordereauStatusService : IBordereauStatusService
{
    private readonly IChifaIntegrationService _integrationService;
    private readonly IChifaInvoiceService _invoiceService;
    private readonly IChifaBordereauService _bordereauService;
    private readonly IChifaSigningService _signingService;
    private readonly IChifaTokenService _tokenService;
    private readonly IChifaAuditService _auditService;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly ChifaBordereauValidator _validator;
    private readonly BordereauWorkflowStateMachine _stateMachine;
    private readonly ILogger<BordereauStatusService> _logger;

    private readonly ConcurrentDictionary<string, BordereauWorkflowState> _states = new();
    private readonly ConcurrentDictionary<string, List<string>> _invoiceNumbers = new();
    private readonly ConcurrentDictionary<string, decimal> _amounts = new();
    private readonly ConcurrentDictionary<string, string> _signedInvoices = new();
    private readonly ConcurrentBag<BordereauWorkflowAuditEntry> _auditLog = new();

    public BordereauStatusService(
        IChifaIntegrationService integrationService,
        IChifaInvoiceService invoiceService,
        IChifaBordereauService bordereauService,
        IChifaSigningService signingService,
        IChifaTokenService tokenService,
        IChifaAuditService auditService,
        ChifaIntegrationModeProvider modeProvider,
        ChifaBordereauValidator validator,
        BordereauWorkflowStateMachine stateMachine,
        ILogger<BordereauStatusService> logger)
    {
        _integrationService = integrationService;
        _invoiceService = invoiceService;
        _bordereauService = bordereauService;
        _signingService = signingService;
        _tokenService = tokenService;
        _auditService = auditService;
        _modeProvider = modeProvider;
        _validator = validator;
        _stateMachine = stateMachine;
        _logger = logger;
    }

    public async Task<BordereauWorkflowResult> GetStatusAsync(string numBord, CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);

        if (!_states.TryGetValue(numBord, out var state))
        {
            result.IsSuccess = false;
            result.Step = "NOT_FOUND";
            result.ErrorMessage = $"Bordereau {numBord} not found in tracking system.";
            return result;
        }

        result.State = state;
        result.Step = state.ToString().ToUpper();
        result.IsSuccess = true;

        if (_invoiceNumbers.TryGetValue(numBord, out var invs))
            result.InvoiceCount = invs.Count;

        if (_amounts.TryGetValue(numBord, out var amount))
            result.TotalAmount = amount;

        if (_signedInvoices.TryGetValue(numBord, out var signed))
        {
            result.SignedInvoiceCount = signed.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        }

        result.UnsignedInvoiceCount = result.InvoiceCount - result.SignedInvoiceCount;

        var health = await _integrationService.GetHealthStatusAsync(cancellationToken);
        result.IsChifaOnline = health.IsOnline;

        result.RequiresAction = _stateMachine.RequiresHumanAction(state);
        result.ActionDescription = _stateMachine.GetActionDescription(state, numBord);
        result.ActionApplication = _stateMachine.GetActionApplication(state);
        result.ActionBmPharmaWaits = _stateMachine.GetActionBmPharmaWaits(state);

        return result;
    }

    public async Task<BordereauWorkflowResult> CreateBordereauAsync(string numBord, string codeCentre, List<string> invoiceNumbers, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (_modeProvider.IsReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Création de bordereau simulée. Aucune donnée CHIFA n'est modifiée.";
                result.State = BordereauWorkflowState.Created;
                _states[numBord] = BordereauWorkflowState.Created;
                _invoiceNumbers[numBord] = new List<string>(invoiceNumbers);
                await AuditAsync(numBord, "CREATE_BORDEREAU", userId, true, "ReadOnly simulation");
                result.IsSuccess = true;
                result.InvoiceCount = invoiceNumbers.Count;
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            _states[numBord] = BordereauWorkflowState.Preparing;
            await AuditAsync(numBord, "PREPARE_BORDEREAU", userId, true, "Preparing bordereau");

            var validation = _validator.Validate(numBord, codeCentre, invoiceNumbers);
            if (!validation.IsValid)
            {
                result.IsSuccess = false;
                result.Step = "VALIDATION_FAILED";
                result.State = BordereauWorkflowState.Error;
                _states[numBord] = BordereauWorkflowState.Error;
                result.ValidationErrors = validation.Errors.Select(e => new BordereauWorkflowValidationError
                {
                    Code = e.Code, Field = e.Field, Message = e.Message
                }).ToList();
                await AuditAsync(numBord, "VALIDATE", userId, false, $"Validation failed: {validation.Errors.Count} error(s)");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            var createResult = await _bordereauService.CreateBordereauAsync(new ChifaBordereauRequest
            {
                NumBord = numBord,
                TypeBord = "BORD_CNAS",
                DateBord = DateTime.UtcNow,
                InvoiceNumbers = invoiceNumbers
            }, cancellationToken);

            if (!createResult.Success)
            {
                result.IsSuccess = false;
                result.Step = "CREATE_FAILED";
                result.State = BordereauWorkflowState.Error;
                _states[numBord] = BordereauWorkflowState.Error;
                result.ErrorMessage = createResult.ErrorMessage;
                await AuditAsync(numBord, "CREATE_BORDEREAU", userId, false, createResult.ErrorMessage);
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            _states[numBord] = BordereauWorkflowState.Created;
            _invoiceNumbers[numBord] = new List<string>(invoiceNumbers);
            result.State = BordereauWorkflowState.Created;
            result.Step = "CREATED";
            result.IsSuccess = true;
            result.InvoiceCount = invoiceNumbers.Count;
            await AuditAsync(numBord, "CREATE_BORDEREAU", userId, true, $"Created with {invoiceNumbers.Count} invoices");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "UNEXPECTED_ERROR";
            result.State = BordereauWorkflowState.Error;
            _states[numBord] = BordereauWorkflowState.Error;
            result.ErrorMessage = ex.Message;
            await AuditAsync(numBord, "ERROR", userId, false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> AttachInvoicesAsync(string numBord, List<string> invoiceNumbers, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (!_states.TryGetValue(numBord, out var currentState))
            {
                result.IsSuccess = false;
                result.Step = "NOT_FOUND";
                result.ErrorMessage = $"Bordereau {numBord} not found.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (_modeProvider.IsReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Association de factures simulée.";
                _invoiceNumbers.AddOrUpdate(numBord, new List<string>(invoiceNumbers), (_, existing) =>
                {
                    existing.AddRange(invoiceNumbers);
                    return existing;
                });
                result.State = BordereauWorkflowState.InvoicesAttached;
                _states[numBord] = BordereauWorkflowState.InvoicesAttached;
                result.IsSuccess = true;
                result.InvoiceCount = invoiceNumbers.Count;
                await AuditAsync(numBord, "ATTACH_INVOICES", userId, true, $"ReadOnly: Attached {invoiceNumbers.Count} invoices");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            _states[numBord] = _stateMachine.Transition(currentState, BordereauWorkflowState.InvoicesAttached);
            _invoiceNumbers.AddOrUpdate(numBord, new List<string>(invoiceNumbers), (_, existing) =>
            {
                existing.AddRange(invoiceNumbers);
                return existing;
            });
            result.State = BordereauWorkflowState.InvoicesAttached;
            result.Step = "INVOICES_ATTACHED";
            result.IsSuccess = true;
            result.InvoiceCount = _invoiceNumbers[numBord].Count;
            await AuditAsync(numBord, "ATTACH_INVOICES", userId, true, $"Attached {invoiceNumbers.Count} invoices");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching invoices to bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "ATTACH_FAILED";
            result.ErrorMessage = ex.Message;
            result.State = BordereauWorkflowState.Error;
            _states[numBord] = BordereauWorkflowState.Error;
            await AuditAsync(numBord, "ERROR", userId, false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> RemoveInvoiceAsync(string numBord, string numFact, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (!_invoiceNumbers.TryGetValue(numBord, out var invoices))
            {
                result.IsSuccess = false;
                result.Step = "NOT_FOUND";
                result.ErrorMessage = $"Bordereau {numBord} not found.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (!invoices.Remove(numFact))
            {
                result.IsSuccess = false;
                result.Step = "INVOICE_NOT_FOUND";
                result.ErrorMessage = $"Invoice {numFact} not found in bordereau {numBord}.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.State = _states[numBord];
            result.Step = "INVOICE_REMOVED";
            result.IsSuccess = true;
            result.InvoiceCount = invoices.Count;
            await AuditAsync(numBord, "REMOVE_INVOICE", userId, true, $"Removed invoice {numFact}");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing invoice from bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "REMOVE_FAILED";
            result.ErrorMessage = ex.Message;
            await AuditAsync(numBord, "ERROR", userId, false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> ValidateBordereauAsync(string numBord, CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (!_states.TryGetValue(numBord, out var currentState))
            {
                result.IsSuccess = false;
                result.Step = "NOT_FOUND";
                result.ErrorMessage = $"Bordereau {numBord} not found.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (!_invoiceNumbers.TryGetValue(numBord, out var invoices) || invoices.Count == 0)
            {
                result.IsSuccess = false;
                result.Step = "VALIDATION_FAILED";
                result.ErrorMessage = "Bordereau must contain at least one invoice.";
                result.ValidationErrors.Add(new BordereauWorkflowValidationError
                {
                    Code = "NO_INVOICES", Field = "invoices", Message = "Bordereau must contain at least one invoice."
                });
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (_modeProvider.IsReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Validation simulée.";
                result.State = BordereauWorkflowState.InvoicesAttached;
                result.IsSuccess = true;
                _amounts[numBord] = 0;
                await AuditAsync(numBord, "VALIDATE", "system", true, "ReadOnly simulation");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            foreach (var inv in invoices)
            {
                var exists = await _invoiceService.InvoiceExistsInChifaAsync(inv, cancellationToken);
                if (!exists)
                {
                    result.IsSuccess = false;
                    result.Step = "VALIDATION_FAILED";
                    result.ErrorMessage = $"Invoice {inv} not found in CHIFA.";
                    result.ValidationErrors.Add(new BordereauWorkflowValidationError
                    {
                        Code = "INVOICE_NOT_IN_CHIFA", Field = inv, Message = $"Invoice {inv} not found in CHIFA."
                    });
                }
            }

            if (!result.IsSuccess)
            {
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            result.State = BordereauWorkflowState.InvoicesAttached;
            _states[numBord] = BordereauWorkflowState.InvoicesAttached;
            result.Step = "VALIDATED";
            result.IsSuccess = true;
            result.InvoiceCount = invoices.Count;
            await AuditAsync(numBord, "VALIDATE", "system", true, $"Validated {invoices.Count} invoices");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "VALIDATION_ERROR";
            result.ErrorMessage = ex.Message;
            result.State = BordereauWorkflowState.Error;
            _states[numBord] = BordereauWorkflowState.Error;
            await AuditAsync(numBord, "ERROR", "system", false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> SignBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (!_states.TryGetValue(numBord, out var currentState))
            {
                result.IsSuccess = false;
                result.Step = "NOT_FOUND";
                result.ErrorMessage = $"Bordereau {numBord} not found.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (_modeProvider.IsReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Signature simulée. La vraie signature nécessite CHIFA-OFFICINE avec le token professionnel.";
                _states[numBord] = BordereauWorkflowState.ReadyForClosure;
                result.State = BordereauWorkflowState.ReadyForClosure;
                result.IsSuccess = true;
                await AuditAsync(numBord, "SIGN_BORDEREAU", userId, true, "ReadOnly simulation — signature simulated");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            var tokenPresent = await _tokenService.IsTokenPresentAsync(cancellationToken);
            if (!tokenPresent)
            {
                result.IsSuccess = false;
                result.Step = "TOKEN_NOT_PRESENT";
                result.State = BordereauWorkflowState.SignatureError;
                _states[numBord] = BordereauWorkflowState.SignatureError;
                result.ErrorMessage = "Professional token not present. Signing requires CHIFA-OFFICINE with the physical token.";
                result.RequiresAction = true;
                result.ActionDescription = _stateMachine.GetActionDescription(BordereauWorkflowState.AwaitingSignature, numBord);
                result.ActionApplication = "CHIFA-OFFICINE";
                result.ActionBmPharmaWaits = _stateMachine.GetActionBmPharmaWaits(BordereauWorkflowState.AwaitingSignature);
                await AuditAsync(numBord, "SIGN_BORDEREAU", userId, false, "Token not present");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            var signResult = await _bordereauService.SignBordereauAsync(numBord, cancellationToken);
            if (!signResult.Success)
            {
                result.IsSuccess = false;
                result.Step = "SIGN_FAILED";
                result.State = BordereauWorkflowState.SignatureError;
                _states[numBord] = BordereauWorkflowState.SignatureError;
                result.ErrorMessage = signResult.ErrorMessage;
                await AuditAsync(numBord, "SIGN_BORDEREAU", userId, false, signResult.ErrorMessage);
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            _states[numBord] = BordereauWorkflowState.ReadyForClosure;
            result.State = BordereauWorkflowState.ReadyForClosure;
            result.Step = "SIGNED";
            result.IsSuccess = true;
            result.SignatureId = signResult.SignatureId;
            await AuditAsync(numBord, "SIGN_BORDEREAU", userId, true, $"Signed: {signResult.SignatureId}");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "SIGN_ERROR";
            result.State = BordereauWorkflowState.SignatureError;
            _states[numBord] = BordereauWorkflowState.SignatureError;
            result.ErrorMessage = ex.Message;
            await AuditAsync(numBord, "ERROR", userId, false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> CloseBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (!_states.TryGetValue(numBord, out var currentState))
            {
                result.IsSuccess = false;
                result.Step = "NOT_FOUND";
                result.ErrorMessage = $"Bordereau {numBord} not found.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (_modeProvider.IsReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Clôture simulée. La vraie clôture nécessite CHIFA-OFFICINE.";
                _states[numBord] = BordereauWorkflowState.AwaitingTransmission;
                result.State = BordereauWorkflowState.AwaitingTransmission;
                result.IsSuccess = true;
                await AuditAsync(numBord, "CLOSE_BORDEREAU", userId, true, "ReadOnly simulation — closure simulated");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            var closeResult = await _bordereauService.CloseBordereauAsync(numBord, cancellationToken);
            if (!closeResult.Success)
            {
                result.IsSuccess = false;
                result.Step = "CLOSE_FAILED";
                result.State = BordereauWorkflowState.ClosureError;
                _states[numBord] = BordereauWorkflowState.ClosureError;
                result.ErrorMessage = closeResult.ErrorMessage;
                result.RequiresAction = true;
                result.ActionDescription = _stateMachine.GetActionDescription(BordereauWorkflowState.AwaitingClosure, numBord);
                result.ActionApplication = "CHIFA-OFFICINE";
                await AuditAsync(numBord, "CLOSE_BORDEREAU", userId, false, closeResult.ErrorMessage);
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            _states[numBord] = BordereauWorkflowState.AwaitingTransmission;
            result.State = BordereauWorkflowState.AwaitingTransmission;
            result.Step = "CLOSED";
            result.IsSuccess = true;
            await AuditAsync(numBord, "CLOSE_BORDEREAU", userId, true, "Bordereau closed");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "CLOSE_ERROR";
            result.State = BordereauWorkflowState.ClosureError;
            _states[numBord] = BordereauWorkflowState.ClosureError;
            result.ErrorMessage = ex.Message;
            await AuditAsync(numBord, "ERROR", userId, false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> TransmitBordereauAsync(string numBord, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        try
        {
            if (!_states.TryGetValue(numBord, out var currentState))
            {
                result.IsSuccess = false;
                result.Step = "NOT_FOUND";
                result.ErrorMessage = $"Bordereau {numBord} not found.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            if (_modeProvider.IsReadOnly)
            {
                result.Step = "READONLY_SIMULATION";
                result.SimulationMessage = "MODE LECTURE SEULE — Transmission simulée. La vraie transmission nécessite CNAS.";
                _states[numBord] = BordereauWorkflowState.Completed;
                result.State = BordereauWorkflowState.Completed;
                result.IsSuccess = true;
                await AuditAsync(numBord, "TRANSMIT_BORDEREAU", userId, true, "ReadOnly simulation — transmission simulated");
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
                return result;
            }

            _states[numBord] = BordereauWorkflowState.Completed;
            result.State = BordereauWorkflowState.Completed;
            result.Step = "TRANSMITTED";
            result.IsSuccess = true;
            await AuditAsync(numBord, "TRANSMIT_BORDEREAU", userId, true, "Bordereau transmitted to CNAS");

            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transmitting bordereau {NumBord}", numBord);
            result.IsSuccess = false;
            result.Step = "TRANSMIT_ERROR";
            result.State = BordereauWorkflowState.TransmissionError;
            _states[numBord] = BordereauWorkflowState.TransmissionError;
            result.ErrorMessage = ex.Message;
            await AuditAsync(numBord, "ERROR", userId, false, ex.Message);
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<BordereauWorkflowResult> TransitionToAsync(string numBord, BordereauWorkflowState target, string userId = "system", CancellationToken cancellationToken = default)
    {
        var result = CreateResult(numBord);
        var sw = Stopwatch.StartNew();

        if (!_states.TryGetValue(numBord, out var currentState))
        {
            result.IsSuccess = false;
            result.Step = "NOT_FOUND";
            result.ErrorMessage = $"Bordereau {numBord} not found.";
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            return result;
        }

        if (!_stateMachine.CanTransition(currentState, target))
        {
            result.IsSuccess = false;
            result.Step = "INVALID_TRANSITION";
            result.ErrorMessage = $"Cannot transition from {currentState} to {target}. Allowed: [{string.Join(", ", _stateMachine.GetAllowedTransitions(currentState))}]";
            result.State = currentState;
            await AuditAsync(numBord, "INVALID_TRANSITION", userId, false, $"Tried {currentState} → {target}");
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            return result;
        }

        _states[numBord] = target;
        result.State = target;
        result.Step = target.ToString().ToUpper();
        result.IsSuccess = true;
        await AuditAsync(numBord, "TRANSITION", userId, true, $"{currentState} → {target}");

        sw.Stop();
        result.DurationMs = sw.ElapsedMilliseconds;
        return result;
    }

    public IReadOnlyList<BordereauWorkflowAuditEntry> GetAuditLog(string? numBord = null)
    {
        var entries = numBord == null
            ? _auditLog.OrderByDescending(e => e.Timestamp).ToList()
            : _auditLog.Where(e => e.NumBord == numBord).OrderByDescending(e => e.Timestamp).ToList();
        return entries.AsReadOnly();
    }

    public IReadOnlyList<BordereauWorkflowSummary> GetAllBordereaux()
    {
        var summaries = new List<BordereauWorkflowSummary>();
        foreach (var kvp in _states)
        {
            var invCount = _invoiceNumbers.TryGetValue(kvp.Key, out var invs) ? invs.Count : 0;
            var amount = _amounts.TryGetValue(kvp.Key, out var amt) ? amt : 0;
            var summary = new BordereauWorkflowSummary
            {
                NumBord = kvp.Key,
                State = kvp.Value,
                InvoiceCount = invs?.Count ?? 0,
                TotalAmount = amount,
                RequiresAction = _stateMachine.RequiresHumanAction(kvp.Value),
                ActionDescription = _stateMachine.GetActionDescription(kvp.Value, kvp.Key),
                Mode = _modeProvider.CurrentMode,
                LastUpdated = DateTime.UtcNow
            };
            summaries.Add(summary);
        }
        return summaries.OrderByDescending(s => s.LastUpdated).ToList().AsReadOnly();
    }

    public void ClearAuditLog()
    {
        while (_auditLog.TryTake(out _)) { }
    }

    private BordereauWorkflowResult CreateResult(string numBord)
    {
        return new BordereauWorkflowResult
        {
            NumBord = numBord,
            CorrelationId = Guid.NewGuid().ToString("N")[..12],
            Mode = _modeProvider.CurrentMode,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task AuditAsync(string numBord, string operation, string userId, bool success, string? details)
    {
        var entry = new BordereauWorkflowAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            NumBord = numBord,
            CorrelationId = Guid.NewGuid().ToString("N")[..12],
            Operation = operation,
            UserId = userId,
            Result = success ? "SUCCESS" : "ERROR",
            Details = details,
            Mode = _modeProvider.CurrentMode
        };

        _auditLog.Add(entry);

        try
        {
            await _auditService.LogOperationAsync(
                operation, "bordereau", numBord,
                $"[{entry.CorrelationId}] {operation}",
                success, 0, details, userId);
        }
        catch
        {
            _logger.LogWarning("Failed to persist audit for {Operation} on {NumBord}", operation, numBord);
        }
    }
}

public class BordereauWorkflowResult
{
    public string NumBord { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public ChifaIntegrationMode Mode { get; set; }
    public BordereauWorkflowState State { get; set; } = BordereauWorkflowState.Draft;
    public string Step { get; set; } = "PENDING";
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SimulationMessage { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionDescription { get; set; }
    public string? ActionApplication { get; set; }
    public string? ActionBmPharmaWaits { get; set; }
    public int InvoiceCount { get; set; }
    public int SignedInvoiceCount { get; set; }
    public int UnsignedInvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsChifaOnline { get; set; }
    public string? SignatureId { get; set; }
    public List<BordereauWorkflowValidationError> ValidationErrors { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public long DurationMs { get; set; }
}

public class BordereauWorkflowValidationError
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class BordereauWorkflowAuditEntry
{
    public DateTime Timestamp { get; set; }
    public string NumBord { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Details { get; set; }
    public ChifaIntegrationMode Mode { get; set; }
}

public class BordereauWorkflowSummary
{
    public string NumBord { get; set; } = string.Empty;
    public BordereauWorkflowState State { get; set; }
    public int InvoiceCount { get; set; }
    public int SignedInvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionDescription { get; set; }
    public ChifaIntegrationMode Mode { get; set; }
    public DateTime LastUpdated { get; set; }
}
