using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BMPharma.UI.ViewModels;

public partial class ChifaInvoicePreparationViewModel : ViewModelBase
{
    private readonly IChifaInvoiceWorkflowService _workflowService;
    private readonly IChifaIntegrationService _integrationService;
    private readonly IChifaTokenService _tokenService;
    private readonly IChifaSigningService _signingService;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly ILogger<ChifaInvoicePreparationViewModel> _logger;

    // --- Invoice Identity ---
    [ObservableProperty] private string _invoiceNumber = string.Empty;
    [ObservableProperty] private string _insuranceNumber = string.Empty;
    [ObservableProperty] private int _centreCode = 11600;
    [ObservableProperty] private DateTime _careDate = DateTime.Today;
    [ObservableProperty] private string _patientName = string.Empty;

    // --- Invoice Lines ---
    public ObservableCollection<InvoiceLineViewModel> InvoiceLines { get; } = new();

    // --- Computed Amounts ---
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private decimal _reimbursementAmount;
    [ObservableProperty] private decimal _patientShare;

    // --- Status ---
    [ObservableProperty] private string _workflowStep = "En attente";
    [ObservableProperty] private string _workflowStepColor = "#757575";
    [ObservableProperty] private string _validationStatus = "";
    [ObservableProperty] private string _validationStatusColor = "#757575";
    [ObservableProperty] private string _chifaStatus = "";
    [ObservableProperty] private string _chifaStatusColor = "#757575";
    [ObservableProperty] private string _connectionStatus = "Vérification...";
    [ObservableProperty] private string _connectionStatusColor = "#FF9800";

    // --- Mode ---
    [ObservableProperty] private string _integrationMode = "ReadOnly";
    [ObservableProperty] private bool _isReadOnly = true;
    [ObservableProperty] private string _modeNotice = "";

    // --- Action Required ---
    [ObservableProperty] private bool _hasRequiredAction;
    [ObservableProperty] private string _requiredActionTitle = "";
    [ObservableProperty] private string _requiredActionDescription = "";
    [ObservableProperty] private string _requiredActionApplication = "";
    [ObservableProperty] private string _requiredActionBmPharmaWaits = "";

    // --- Errors & Messages ---
    [ObservableProperty] private bool _hasErrors;
    [ObservableProperty] private ObservableCollection<WorkflowErrorViewModel> _errors = new();
    [ObservableProperty] private string _statusMessage = "Prêt";
    [ObservableProperty] private bool _isProcessing;

    // --- Workflow State ---
    [ObservableProperty] private ChifaWorkflowState _currentState = ChifaWorkflowState.Draft;
    [ObservableProperty] private string _currentStateDescription = "Brouillon";
    [ObservableProperty] private string _lastCorrelationId = "";

    // --- Audit ---
    public ObservableCollection<WorkflowAuditViewModel> AuditEntries { get; } = new();

    // --- Computed ---
    [ObservableProperty] private string _numFactDisplay = "-";
    [ObservableProperty] private string _nextBordereauNumber = "-";

    public ChifaInvoicePreparationViewModel(
        IChifaInvoiceWorkflowService workflowService,
        IChifaIntegrationService integrationService,
        IChifaTokenService tokenService,
        IChifaSigningService signingService,
        ChifaIntegrationModeProvider modeProvider,
        ILogger<ChifaInvoicePreparationViewModel> logger)
    {
        _workflowService = workflowService;
        _integrationService = integrationService;
        _tokenService = tokenService;
        _signingService = signingService;
        _modeProvider = modeProvider;
        _logger = logger;

        Title = "Préparation facture CHIFA";
        IntegrationMode = _modeProvider.CurrentMode.ToString();
        IsReadOnly = _modeProvider.IsReadOnly;

        ModeNotice = IsReadOnly
            ? "MODE LECTURE SEULE — Cette opération est simulée. Aucune donnée CHIFA n'est modifiée."
            : "Mode actif — Les opérations seront exécutées sur la base CHIFA.";

        InvoiceLines.CollectionChanged += (_, _) => RecalculateAmounts();
        _ = LoadStatusAsync();
    }

    // --- Commands ---

    [RelayCommand]
    private void AddLine()
    {
        InvoiceLines.Add(new InvoiceLineViewModel
        {
            LineNumber = InvoiceLines.Count + 1,
            NumEnr = (InvoiceLines.Count + 1).ToString().PadLeft(5, '0'),
            Quantity = 1,
            UnitPrice = 0
        });
        RecalculateAmounts();
    }

    [RelayCommand]
    private void RemoveLine(InvoiceLineViewModel? line)
    {
        if (line != null)
        {
            InvoiceLines.Remove(line);
            for (int i = 0; i < InvoiceLines.Count; i++)
                InvoiceLines[i].LineNumber = i + 1;
            RecalculateAmounts();
        }
    }

    [RelayCommand]
    private async Task ValidateInvoiceAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Validation en cours...";
            HasErrors = false;
            Errors.Clear();

            var request = BuildChifaRequest();
            var result = await _workflowService.ValidateOnlyAsync(request);

            LastCorrelationId = result.CorrelationId;

            if (result.IsSuccess)
            {
                ValidationStatus = "Validation réussie";
                ValidationStatusColor = "#4CAF50";
                TotalAmount = result.ComputedMontFact;
                ReimbursementAmount = result.ComputedMontAs;
                PatientShare = result.ComputedMontMut;
                WorkflowStep = "Validé — Prêt pour préparation";
                WorkflowStepColor = "#4CAF50";
                CurrentState = ChifaWorkflowState.Validated;
                CurrentStateDescription = "Validé";
                StatusMessage = $"Validation réussie en {result.DurationMs}ms";
            }
            else
            {
                ValidationStatus = $"Échec — {result.ValidationErrors.Count} erreur(s)";
                ValidationStatusColor = "#F44336";
                foreach (var error in result.ValidationErrors)
                {
                    Errors.Add(new WorkflowErrorViewModel
                    {
                        Code = error.Code,
                        Field = error.Field,
                        Message = error.Message
                    });
                }
                HasErrors = true;
                WorkflowStep = "Échec de validation";
                WorkflowStepColor = "#F44336";
                StatusMessage = $"Validation échouée: {result.ValidationErrors.Count} erreur(s)";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during invoice validation");
            HasErrors = true;
            Errors.Add(new WorkflowErrorViewModel
            {
                Code = "EXCEPTION",
                Field = "system",
                Message = $"Erreur inattendue: {ex.Message}"
            });
            StatusMessage = "Erreur lors de la validation";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task PrepareAndSubmitAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Préparation et soumission en cours...";
            HasErrors = false;
            Errors.Clear();

            var request = BuildChifaRequest();
            var result = await _workflowService.ExecuteFullWorkflowAsync(request);

            LastCorrelationId = result.CorrelationId;
            CurrentState = result.State;
            CurrentStateDescription = GetStateDescription(result.State);

            if (result.IsSuccess)
            {
                WorkflowStep = result.Step;
                WorkflowStepColor = "#4CAF50";
                ChifaStatus = result.State == ChifaWorkflowState.VisibleInChifa ? "Visible" : result.Step;
                ChifaStatusColor = "#4CAF50";
                NumFactDisplay = result.ChifaNumFact ?? InvoiceNumber;

                if (!string.IsNullOrEmpty(result.SimulationMessage))
                {
                    StatusMessage = result.SimulationMessage;
                    WorkflowStepColor = "#FF9800";
                    ChifaStatus = "Simulation";
                    ChifaStatusColor = "#FF9800";
                }
                else
                {
                    StatusMessage = $"Étape {result.Step} complétée en {result.DurationMs}ms";
                }

                if (result.RequiresAction)
                {
                    HasRequiredAction = true;
                    RequiredActionTitle = result.ActionDescription ?? "";
                    RequiredActionDescription = result.ActionBmPharmaWaits ?? "";
                    RequiredActionApplication = result.ActionApplication ?? "CHIFA-OFFICINE";
                    RequiredActionBmPharmaWaits = result.ActionBmPharmaWaits ?? "";
                }
            }
            else
            {
                WorkflowStep = result.Step;
                WorkflowStepColor = "#F44336";
                HasErrors = true;

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Errors.Add(new WorkflowErrorViewModel
                    {
                        Code = result.Step,
                        Field = "workflow",
                        Message = result.ErrorMessage
                    });
                }

                StatusMessage = $"Échec: {result.Step}";
            }

            RefreshAuditLog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during invoice preparation");
            HasErrors = true;
            Errors.Add(new WorkflowErrorViewModel
            {
                Code = "EXCEPTION",
                Field = "system",
                Message = $"Erreur inattendue: {ex.Message}"
            });
            StatusMessage = "Erreur lors de la préparation";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        await LoadStatusAsync();
    }

    [RelayCommand]
    private void ClearForm()
    {
        InvoiceNumber = string.Empty;
        InsuranceNumber = string.Empty;
        CentreCode = 11600;
        CareDate = DateTime.Today;
        PatientName = string.Empty;
        InvoiceLines.Clear();
        TotalAmount = 0;
        ReimbursementAmount = 0;
        PatientShare = 0;
        ValidationStatus = "";
        ChifaStatus = "";
        WorkflowStep = "En attente";
        WorkflowStepColor = "#757575";
        CurrentState = ChifaWorkflowState.Draft;
        CurrentStateDescription = "Brouillon";
        HasErrors = false;
        Errors.Clear();
        HasRequiredAction = false;
        StatusMessage = "Formulaire réinitialisé";
        NumFactDisplay = "-";
    }

    // --- Private Methods ---

    private async Task LoadStatusAsync()
    {
        try
        {
            IsLoading = true;
            var health = await _integrationService.GetHealthStatusAsync();
            ConnectionStatus = health.IsOnline ? "Connecté" : "Déconnecté";
            ConnectionStatusColor = health.IsOnline ? "#4CAF50" : "#F44336";

            var tokenPresent = await _tokenService.IsTokenPresentAsync();
            var signingStatus = await _signingService.GetSigningStatusAsync("_global");

            StatusMessage = $"Prêt — Dernière vérification: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            ConnectionStatus = "Erreur";
            ConnectionStatusColor = "#F44336";
            StatusMessage = "Erreur de connexion";
            _logger.LogWarning(ex, "Failed to load status");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private ChifaInvoiceRequest BuildChifaRequest()
    {
        return new ChifaInvoiceRequest
        {
            NumFact = InvoiceNumber,
            NumAssure = InsuranceNumber,
            CodeCentre = CentreCode,
            DateSoin = CareDate,
            Lines = InvoiceLines.Select(l => new ChifaInvoiceLineRequest
            {
                NumEnr = l.NumEnr,
                MedicCode = l.MedicCode,
                PrixUnit = l.UnitPrice,
                Quantite = l.Quantity
            }).ToList()
        };
    }

    private void RecalculateAmounts()
    {
        TotalAmount = InvoiceLines.Sum(l => l.Quantity * l.UnitPrice);
        ReimbursementAmount = Math.Round(TotalAmount * 0.70m, 2);
        PatientShare = Math.Round(TotalAmount - ReimbursementAmount, 2);
    }

    private void RefreshAuditLog()
    {
        AuditEntries.Clear();
        foreach (var entry in _workflowService.GetAuditLog().Take(20))
        {
            AuditEntries.Add(new WorkflowAuditViewModel
            {
                Timestamp = entry.Timestamp.ToString("HH:mm:ss.fff"),
                Operation = entry.Operation,
                EntityType = entry.EntityType ?? "",
                EntityKey = entry.EntityKey ?? "",
                Source = entry.Source ?? "",
                Destination = entry.Destination ?? "",
                Result = entry.Result,
                Details = entry.Details ?? ""
            });
        }
    }

    private static string GetStateDescription(ChifaWorkflowState state) => state switch
    {
        ChifaWorkflowState.Draft => "Brouillon",
        ChifaWorkflowState.Validated => "Validé",
        ChifaWorkflowState.PreparedForChifa => "Préparé pour CHIFA",
        ChifaWorkflowState.WrittenToChifa => "Écrit dans CHIFA",
        ChifaWorkflowState.VisibleInChifa => "Visible dans CHIFA",
        ChifaWorkflowState.Signed => "Signé",
        ChifaWorkflowState.BordereauAssigned => "Bordereau assigné",
        ChifaWorkflowState.BordereauClosed => "Bordereau clôturé",
        ChifaWorkflowState.Transmitted => "Transmis à la CNAS",
        ChifaWorkflowState.Failed => "Échoué",
        ChifaWorkflowState.Rejected => "Rejeté",
        ChifaWorkflowState.RollbackRequired => "Rollback requis",
        ChifaWorkflowState.Cancelled => "Annulé",
        _ => "Inconnu"
    };
}

public partial class InvoiceLineViewModel : ObservableObject
{
    [ObservableProperty] private int _lineNumber;
    [ObservableProperty] private string _numEnr = string.Empty;
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string _cipCode = string.Empty;
    [ObservableProperty] private int _medicCode;
    [ObservableProperty] private int _quantity = 1;
    [ObservableProperty] private decimal _unitPrice;

    public decimal LineTotal => Quantity * UnitPrice;
}

public partial class WorkflowErrorViewModel : ObservableObject
{
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string _field = string.Empty;
    [ObservableProperty] private string _message = string.Empty;
}

public partial class WorkflowAuditViewModel : ObservableObject
{
    [ObservableProperty] private string _timestamp = string.Empty;
    [ObservableProperty] private string _operation = string.Empty;
    [ObservableProperty] private string _entityType = string.Empty;
    [ObservableProperty] private string _entityKey = string.Empty;
    [ObservableProperty] private string _source = string.Empty;
    [ObservableProperty] private string _destination = string.Empty;
    [ObservableProperty] private string _result = string.Empty;
    [ObservableProperty] private string _details = string.Empty;
}
