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
    private readonly IChifaIntegrationFacade _facade;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly ILogger<ChifaInvoicePreparationViewModel> _logger;

    [ObservableProperty] private string _invoiceNumber = string.Empty;
    [ObservableProperty] private string _insuranceNumber = string.Empty;
    [ObservableProperty] private int _centreCode = 11600;
    [ObservableProperty] private DateTime _careDate = DateTime.Today;
    [ObservableProperty] private string _patientName = string.Empty;

    public ObservableCollection<InvoiceLineViewModel> InvoiceLines { get; } = new();

    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private decimal _reimbursementAmount;
    [ObservableProperty] private decimal _patientShare;

    [ObservableProperty] private string _workflowStep = "En attente";
    [ObservableProperty] private string _workflowStepColor = "#757575";
    [ObservableProperty] private string _validationStatus = "";
    [ObservableProperty] private string _validationStatusColor = "#757575";
    [ObservableProperty] private string _chifaStatus = "";
    [ObservableProperty] private string _chifaStatusColor = "#757575";
    [ObservableProperty] private string _connectionStatus = "Vérification...";
    [ObservableProperty] private string _connectionStatusColor = "#FF9800";

    [ObservableProperty] private string _integrationMode = "ReadOnly";
    [ObservableProperty] private bool _isReadOnly = true;
    [ObservableProperty] private string _modeNotice = "";

    [ObservableProperty] private bool _hasRequiredAction;
    [ObservableProperty] private string _requiredActionTitle = "";
    [ObservableProperty] private string _requiredActionDescription = "";
    [ObservableProperty] private string _requiredActionApplication = "";
    [ObservableProperty] private string _requiredActionBmPharmaWaits = "";

    [ObservableProperty] private bool _hasErrors;
    [ObservableProperty] private ObservableCollection<WorkflowErrorViewModel> _errors = new();
    [ObservableProperty] private string _statusMessage = "Prêt";
    [ObservableProperty] private bool _isProcessing;

    [ObservableProperty] private ChifaWorkflowState _currentState = ChifaWorkflowState.Draft;
    [ObservableProperty] private string _currentStateDescription = "Brouillon";
    [ObservableProperty] private string _lastCorrelationId = "";

    public ObservableCollection<WorkflowAuditViewModel> AuditEntries { get; } = new();

    [ObservableProperty] private string _numFactDisplay = "-";
    [ObservableProperty] private string _nextBordereauNumber = "-";

    [ObservableProperty] private bool _showPreview;
    [ObservableProperty] private string _previewTitle = "";
    [ObservableProperty] private string _previewLinesSummary = "";
    [ObservableProperty] private string _previewValidationRules = "";
    [ObservableProperty] private string _previewModeNotice = "";
    [ObservableProperty] private string _previewTotal = "";
    [ObservableProperty] private string _previewReimbursement = "";
    [ObservableProperty] private string _previewPatientShare = "";

    public ChifaInvoicePreparationViewModel(
        IChifaIntegrationFacade facade,
        ChifaIntegrationModeProvider modeProvider,
        ILogger<ChifaInvoicePreparationViewModel> logger)
    {
        _facade = facade;
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
    private void PreviewInvoice()
    {
        if (InvoiceLines.Count == 0)
        {
            StatusMessage = "Ajoutez au moins une ligne de facture.";
            return;
        }

        ShowPreview = true;
        PreviewTitle = $"Aperçu — Facture {InvoiceNumber}";
        PreviewLinesSummary = $"{InvoiceLines.Count} ligne(s) de facture";
        PreviewValidationRules = TotalAmount > 0
            ? "✓ Montant valide"
            : "⚠ Le montant total est nul";
        PreviewModeNotice = IsReadOnly
            ? "⚠ Mode lecture seule — Aucune écriture"
            : "✓ Mode actif — Écritures autorisées";
        PreviewTotal = $"{TotalAmount:N2} DA";
        PreviewReimbursement = $"{ReimbursementAmount:N2} DA";
        PreviewPatientShare = $"{PatientShare:N2} DA";
        StatusMessage = "Aperçu affiché — Vérifiez avant de soumettre";
    }

    [RelayCommand]
    private void HidePreview()
    {
        ShowPreview = false;
        StatusMessage = "Aperçu masqué";
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

            if (IsReadOnly)
            {
                AddSimulatedError("READONLY", "system",
                    "La validation est simulée en mode lecture seule.");
                StatusMessage = "Validation simulée (mode lecture seule)";
                return;
            }

            var request = BuildChifaRequest();
            var result = await _facade.ValidateInvoiceAsync(request);

            LastCorrelationId = result.Data?.CorrelationId ?? "";

            if (result.IsSuccess)
            {
                ValidationStatus = "Validation réussie";
                ValidationStatusColor = "#4CAF50";
                TotalAmount = result.Data?.ComputedMontFact ?? 0;
                ReimbursementAmount = result.Data?.ComputedMontAs ?? 0;
                PatientShare = result.Data?.ComputedMontMut ?? 0;
                WorkflowStep = "Validé — Prêt pour préparation";
                WorkflowStepColor = "#4CAF50";
                CurrentState = ChifaWorkflowState.Validated;
                CurrentStateDescription = "Validé";
                StatusMessage = $"Validation réussie en {result.DurationMs}ms";
            }
            else
            {
                var errorList = result.Data?.ValidationErrors ?? new List<ChifaWorkflowValidationError>();
                ValidationStatus = $"Échec — {errorList.Count} erreur(s)";
                ValidationStatusColor = "#F44336";
                foreach (var error in errorList)
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
                StatusMessage = $"Validation échouée: {errorList.Count} erreur(s)";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during invoice validation");
            AddSimulatedError(ex.GetErrorCode(), "system", ex.ToUserMessage());
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

            if (IsReadOnly)
            {
                AddSimulatedError("READONLY", "system",
                    "La soumission est simulée en mode lecture seule.");
                StatusMessage = "Soumission simulée (mode lecture seule)";
                ShowPreview = false;
                return;
            }

            var request = BuildChifaRequest();
            var result = await _facade.ExecuteFullWorkflowAsync(request);

            LastCorrelationId = result.Data?.CorrelationId ?? "";
            CurrentState = result.Data?.State ?? ChifaWorkflowState.Draft;
            CurrentStateDescription = GetStateDescription(CurrentState);

            if (result.IsSuccess)
            {
                WorkflowStep = result.Data?.Step ?? "COMPLETED";
                WorkflowStepColor = "#4CAF50";
                ChifaStatus = result.Data?.State == ChifaWorkflowState.PreparedForChifa ? "Préparé" : result.Data?.Step ?? "";
                ChifaStatusColor = "#4CAF50";
                NumFactDisplay = result.Data?.ChifaNumFact ?? InvoiceNumber;

                if (!string.IsNullOrEmpty(result.Data?.SimulationMessage))
                {
                    StatusMessage = result.Data.SimulationMessage;
                    WorkflowStepColor = "#FF9800";
                    ChifaStatus = "Simulation";
                    ChifaStatusColor = "#FF9800";
                }
                else
                {
                    StatusMessage = $"Étape {result.Data?.Step} complétée en {result.DurationMs}ms";
                }

                if (result.Data?.RequiresAction == true)
                {
                    HasRequiredAction = true;
                    RequiredActionTitle = result.Data.ActionDescription ?? "";
                    RequiredActionDescription = result.Data.ActionBmPharmaWaits ?? "";
                    RequiredActionApplication = result.Data.ActionApplication ?? "CHIFA-OFFICINE";
                    RequiredActionBmPharmaWaits = result.Data.ActionBmPharmaWaits ?? "";
                }
            }
            else
            {
                WorkflowStep = result.Data?.Step ?? "FAILED";
                WorkflowStepColor = "#F44336";
                HasErrors = true;

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Errors.Add(new WorkflowErrorViewModel
                    {
                        Code = result.Data?.Step ?? "UNKNOWN",
                        Field = "workflow",
                        Message = result.ErrorMessage
                    });
                }

                StatusMessage = $"Échec: {result.Data?.Step ?? "UNKNOWN"}";
            }

            ShowPreview = false;
            RefreshAuditLog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during invoice preparation");
            AddSimulatedError(ex.GetErrorCode(), "system", ex.ToUserMessage());
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
        ShowPreview = false;
        StatusMessage = "Formulaire réinitialisé";
        NumFactDisplay = "-";
    }

    private async Task LoadStatusAsync()
    {
        try
        {
            IsLoading = true;
            var overview = await _facade.GetDashboardOverviewAsync();
            var health = await _facade.GetHealthStatusAsync();

            ConnectionStatus = health.IsOnline ? "Connecté" : "Déconnecté";
            ConnectionStatusColor = health.IsOnline ? "#4CAF50" : "#F44336";

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

    private async void RefreshAuditLog()
    {
        AuditEntries.Clear();
        var entries = await _facade.GetWorkflowAuditLogAsync(20);
        foreach (var entry in entries)
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

    private void AddSimulatedError(string code, string field, string message)
    {
        HasErrors = true;
        Errors.Add(new WorkflowErrorViewModel
        {
            Code = code,
            Field = field,
            Message = message
        });
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
