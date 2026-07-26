using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BMPharma.UI.ViewModels;

public partial class ChifaBordereauStatusViewModel : ViewModelBase
{
    private readonly IBordereauStatusService _bordereauService;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly IChifaTokenService _tokenService;
    private readonly IChifaIntegrationService _integrationService;
    private readonly ILogger<ChifaBordereauStatusViewModel> _logger;

    // --- Mode ---
    [ObservableProperty] private string _integrationMode = "ReadOnly";
    [ObservableProperty] private string _modeNotice = "";

    // --- Connection ---
    [ObservableProperty] private string _connectionStatus = "Vérification...";
    [ObservableProperty] private string _connectionStatusColor = "#FF9800";

    // --- Token ---
    [ObservableProperty] private string _tokenStatus = "Vérification...";
    [ObservableProperty] private string _tokenStatusColor = "#FF9800";

    // --- Bordereau List ---
    public ObservableCollection<BordereauWorkflowSummary> Bordereaux { get; } = new();

    [ObservableProperty] private BordereauWorkflowSummary? _selectedBordereau;

    // --- Computed Properties for Selected Bordereau ---
    public string NumBord => SelectedBordereau?.NumBord ?? "-";
    public string CurrentState => SelectedBordereau?.State.ToString() ?? "-";
    public int InvoiceCount => SelectedBordereau?.InvoiceCount ?? 0;
    public int SignedInvoiceCount => SelectedBordereau?.SignedInvoiceCount ?? 0;
    public int UnsignedInvoiceCount => InvoiceCount - SignedInvoiceCount;
    public decimal TotalAmount => SelectedBordereau?.TotalAmount ?? 0;
    public bool RequiresAction => SelectedBordereau?.RequiresAction ?? false;
    public string ActionDescription => SelectedBordereau?.ActionDescription ?? "";
    public string ActionApplication => "";
    public string ActionBmPharmaWaits => "";
    public bool IsSimulationMode => _modeProvider.IsReadOnly;
    public string SimulationMessage => IsSimulationMode
        ? "MODE LECTURE SEULE — Cette opération est simulée. Aucune donnée CHIFA n'est modifiée."
        : "Mode actif — Les opérations seront exécutées sur la base CHIFA.";

    // --- Audit Log ---
    public ObservableCollection<BordereauWorkflowAuditEntry> AuditLog { get; } = new();

    // --- Validation Errors ---
    public ObservableCollection<BordereauWorkflowValidationError> ValidationErrors { get; } = new();

    // --- Status ---
    [ObservableProperty] private string _statusMessage = "Prêt";
    [ObservableProperty] private bool _isProcessing;

    // --- Errors ---
    [ObservableProperty] private bool _hasErrors;

    // --- Alerts Count ---
    [ObservableProperty] private int _alertsCount;

    public ChifaBordereauStatusViewModel(
        IBordereauStatusService bordereauService,
        ChifaIntegrationModeProvider modeProvider,
        IChifaTokenService tokenService,
        IChifaIntegrationService integrationService,
        ILogger<ChifaBordereauStatusViewModel> logger)
    {
        _bordereauService = bordereauService;
        _modeProvider = modeProvider;
        _tokenService = tokenService;
        _integrationService = integrationService;
        _logger = logger;

        Title = "Suivi des bordereaux CHIFA";
        IntegrationMode = _modeProvider.CurrentMode.ToString();
        ModeNotice = _modeProvider.IsReadOnly
            ? "MODE LECTURE SEULE — Cette opération est simulée. Aucune donnée CHIFA n'est modifiée."
            : "Mode actif — Les opérations seront exécutées sur la base CHIFA.";

        _ = LoadBordereauxAsync();
    }

    partial void OnSelectedBordereauChanged(BordereauWorkflowSummary? value)
    {
        OnPropertyChanged(nameof(NumBord));
        OnPropertyChanged(nameof(CurrentState));
        OnPropertyChanged(nameof(InvoiceCount));
        OnPropertyChanged(nameof(SignedInvoiceCount));
        OnPropertyChanged(nameof(UnsignedInvoiceCount));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(RequiresAction));
        OnPropertyChanged(nameof(ActionDescription));
        OnPropertyChanged(nameof(ActionApplication));
        OnPropertyChanged(nameof(ActionBmPharmaWaits));
    }

    // --- Commands ---

    [RelayCommand]
    private async Task RefreshBordereauxAsync()
    {
        await LoadBordereauxAsync();
    }

    [RelayCommand]
    private async Task CreateBordereauAsync(string? parameters)
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "Création du bordereau en cours...";
            HasErrors = false;
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(parameters))
            {
                StatusMessage = "Paramètres manquants. Format: numBord;codeCentre;facture1,facture2,...";
                return;
            }

            var parts = parameters.Split(';', StringSplitOptions.TrimEntries);
            if (parts.Length < 3)
            {
                StatusMessage = "Format attendu: numBord;codeCentre;facture1,facture2,...";
                return;
            }

            var numBord = parts[0];
            var codeCentre = parts[1];
            var invoiceNumbersStr = parts[2];
            var invoiceNumbers = invoiceNumbersStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            var result = await _bordereauService.CreateBordereauAsync(numBord, codeCentre, invoiceNumbers);

            if (result.IsSuccess)
            {
                StatusMessage = !string.IsNullOrEmpty(result.SimulationMessage)
                    ? result.SimulationMessage
                    : $"Bordereau {numBord} créé avec {invoiceNumbers.Count} facture(s) en {result.DurationMs}ms";
            }
            else
            {
                StatusMessage = $"Échec de création: {result.ErrorMessage}";
                HasErrors = true;
                foreach (var error in result.ValidationErrors)
                {
                    ValidationErrors.Add(error);
                }
            }

            await LoadBordereauxAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bordereau");
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ValidateBordereauAsync()
    {
        try
        {
            if (SelectedBordereau == null)
            {
                StatusMessage = "Sélectionnez un bordereau à valider.";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Validation du bordereau en cours...";
            HasErrors = false;
            ValidationErrors.Clear();

            var result = await _bordereauService.ValidateBordereauAsync(SelectedBordereau.NumBord);

            if (result.IsSuccess)
            {
                StatusMessage = !string.IsNullOrEmpty(result.SimulationMessage)
                    ? result.SimulationMessage
                    : $"Bordereau {SelectedBordereau.NumBord} validé en {result.DurationMs}ms";
            }
            else
            {
                StatusMessage = $"Validation échouée: {result.ErrorMessage}";
                HasErrors = true;
                foreach (var error in result.ValidationErrors)
                {
                    ValidationErrors.Add(error);
                }
            }

            await LoadBordereauxAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating bordereau");
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SignBordereauAsync()
    {
        try
        {
            if (SelectedBordereau == null)
            {
                StatusMessage = "Sélectionnez un bordereau à signer.";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Signature du bordereau en cours...";
            HasErrors = false;
            ValidationErrors.Clear();

            var result = await _bordereauService.SignBordereauAsync(SelectedBordereau.NumBord);

            if (result.IsSuccess)
            {
                StatusMessage = !string.IsNullOrEmpty(result.SimulationMessage)
                    ? result.SimulationMessage
                    : $"Bordereau {SelectedBordereau.NumBord} signé en {result.DurationMs}ms";
            }
            else
            {
                StatusMessage = $"Signature échouée: {result.ErrorMessage}";
            }

            await LoadBordereauxAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing bordereau");
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task CloseBordereauAsync()
    {
        try
        {
            if (SelectedBordereau == null)
            {
                StatusMessage = "Sélectionnez un bordereau à clôturer.";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Clôture du bordereau en cours...";
            HasErrors = false;
            ValidationErrors.Clear();

            var result = await _bordereauService.CloseBordereauAsync(SelectedBordereau.NumBord);

            if (result.IsSuccess)
            {
                StatusMessage = !string.IsNullOrEmpty(result.SimulationMessage)
                    ? result.SimulationMessage
                    : $"Bordereau {SelectedBordereau.NumBord} clôturé en {result.DurationMs}ms";
            }
            else
            {
                StatusMessage = $"Clôture échouée: {result.ErrorMessage}";
            }

            await LoadBordereauxAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing bordereau");
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task TransmitBordereauAsync()
    {
        try
        {
            if (SelectedBordereau == null)
            {
                StatusMessage = "Sélectionnez un bordereau à transmettre.";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Transmission du bordereau en cours...";
            HasErrors = false;
            ValidationErrors.Clear();

            var result = await _bordereauService.TransmitBordereauAsync(SelectedBordereau.NumBord);

            if (result.IsSuccess)
            {
                StatusMessage = !string.IsNullOrEmpty(result.SimulationMessage)
                    ? result.SimulationMessage
                    : $"Bordereau {SelectedBordereau.NumBord} transmis en {result.DurationMs}ms";
            }
            else
            {
                StatusMessage = $"Transmission échouée: {result.ErrorMessage}";
            }

            await LoadBordereauxAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transmitting bordereau");
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private Task LoadAuditLogAsync()
    {
        try
        {
            AuditLog.Clear();
            var entries = _bordereauService.GetAuditLog(SelectedBordereau?.NumBord);
            foreach (var entry in entries)
            {
                AuditLog.Add(entry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load audit log");
            StatusMessage = "Erreur lors du chargement du journal d'audit";
        }
        return Task.CompletedTask;
    }

    // --- Private Methods ---

    private async Task LoadBordereauxAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Chargement des bordereaux...";

            var health = await _integrationService.GetHealthStatusAsync();
            ConnectionStatus = health.IsOnline ? "Connecté" : "Déconnecté";
            ConnectionStatusColor = health.IsOnline ? "#4CAF50" : "#F44336";

            var tokenPresent = await _tokenService.IsTokenPresentAsync();
            TokenStatus = tokenPresent ? "Présent" : "Absent";
            TokenStatusColor = tokenPresent ? "#4CAF50" : "#F44336";

            Bordereaux.Clear();
            var summaries = _bordereauService.GetAllBordereaux();
            foreach (var summary in summaries)
            {
                Bordereaux.Add(summary);
            }

            AlertsCount = Bordereaux.Count(b => b.RequiresAction);

            LoadAuditLogInternal();

            StatusMessage = $"Prêt — {Bordereaux.Count} bordereau(x) — Dernière vérification: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            ConnectionStatus = "Erreur";
            ConnectionStatusColor = "#F44336";
            TokenStatus = "Erreur";
            TokenStatusColor = "#F44336";
            StatusMessage = "Erreur lors du chargement";
            _logger.LogWarning(ex, "Failed to load bordereaux");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadAuditLogInternal()
    {
        AuditLog.Clear();
        var entries = _bordereauService.GetAuditLog(SelectedBordereau?.NumBord);
        foreach (var entry in entries.Take(50))
        {
            AuditLog.Add(entry);
        }
    }
}
