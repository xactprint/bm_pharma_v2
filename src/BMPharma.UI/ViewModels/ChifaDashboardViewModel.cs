using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BMPharma.UI.ViewModels;

public partial class ChifaDashboardViewModel : ViewModelBase
{
    private readonly IChifaIntegrationService _integrationService;
    private readonly IChifaTokenService _tokenService;
    private readonly IChifaSigningService _signingService;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly ILogger<ChifaDashboardViewModel> _logger;

    public ChifaDashboardViewModel(
        IChifaIntegrationService integrationService,
        IChifaTokenService tokenService,
        IChifaSigningService signingService,
        ChifaIntegrationModeProvider modeProvider,
        ILogger<ChifaDashboardViewModel> logger)
    {
        _integrationService = integrationService;
        _tokenService = tokenService;
        _signingService = signingService;
        _modeProvider = modeProvider;
        _logger = logger;

        Title = "Tableau de bord CHIFA";
        IntegrationMode = _modeProvider.CurrentMode.ToString();
        IsReadOnly = _modeProvider.IsReadOnly;

        _ = LoadStatusAsync();
    }

    // --- Connection ---
    [ObservableProperty] private string _connectionStatus = "Vérification...";
    [ObservableProperty] private string _connectionStatusColor = "#FF9800";
    [ObservableProperty] private bool _isPostgresConnected;

    // --- Mode ---
    [ObservableProperty] private string _integrationMode = "ReadOnly";
    [ObservableProperty] private bool _isReadOnly = true;
    [ObservableProperty] private string _modeDescription = "";

    // --- CHIFA-OFFICINE ---
    [ObservableProperty] private string _chifaStatus = "Vérification...";
    [ObservableProperty] private string _chifaStatusColor = "#FF9800";
    [ObservableProperty] private bool _isChifaAvailable;

    // --- Token ---
    [ObservableProperty] private string _tokenStatus = "Vérification...";
    [ObservableProperty] private string _tokenStatusColor = "#FF9800";
    [ObservableProperty] private bool _isTokenPresent;

    // --- Signature ---
    [ObservableProperty] private string _signingStatus = "Non vérifié";
    [ObservableProperty] private string _signingStatusColor = "#757575";

    // --- Business counts ---
    [ObservableProperty] private int _preparedInvoiceCount;
    [ObservableProperty] private int _synchronizedInvoiceCount;
    [ObservableProperty] private int _bordereauPreparedCount;
    [ObservableProperty] private string _nextBordereauNumber = "-";

    // --- CNAS ---
    [ObservableProperty] private string _cnasTransmissionStatus = "En attente";
    [ObservableProperty] private string _cnasTransmissionColor = "#757575";

    // --- Audit ---
    [ObservableProperty] private string _lastOperation = "Aucune";
    [ObservableProperty] private string _lastOperationTime = "-";

    // --- Error ---
    [ObservableProperty] private string _lastError = "";
    [ObservableProperty] private bool _hasError;

    // --- Loading ---
    [ObservableProperty] private string _statusMessage = "Chargement en cours...";

    // --- Workflow Steps (business-oriented) ---
    [ObservableProperty] private string _workflowStep1Status = "En attente";
    [ObservableProperty] private string _workflowStep1Color = "#757575";
    [ObservableProperty] private string _workflowStep2Status = "En attente";
    [ObservableProperty] private string _workflowStep2Color = "#757575";
    [ObservableProperty] private string _workflowStep3Status = "En attente";
    [ObservableProperty] private string _workflowStep3Color = "#757575";
    [ObservableProperty] private string _workflowStep4Status = "En attente";
    [ObservableProperty] private string _workflowStep4Color = "#757575";
    [ObservableProperty] private string _workflowStep5Status = "En attente";
    [ObservableProperty] private string _workflowStep5Color = "#757575";
    [ObservableProperty] private string _workflowStep6Status = "En attente";
    [ObservableProperty] private string _workflowStep6Color = "#757575";
    [ObservableProperty] private string _workflowStep7Status = "En attente";
    [ObservableProperty] private string _workflowStep7Color = "#757575";
    [ObservableProperty] private string _workflowStep8Status = "En attente";
    [ObservableProperty] private string _workflowStep8Color = "#757575";

    // --- Required Action ---
    [ObservableProperty] private bool _hasRequiredAction;
    [ObservableProperty] private string _requiredActionTitle = "";
    [ObservableProperty] private string _requiredActionDescription = "";
    [ObservableProperty] private string _requiredActionApplication = "";
    [ObservableProperty] private string _requiredActionBmPharmaWaits = "";

    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        await LoadStatusAsync();
    }

    private async Task LoadStatusAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            LastError = "";
            StatusMessage = "Vérification de l'état du système...";

            LoadModeStatusAsync();
            await LoadConnectionStatusAsync();
            await LoadTokenStatusAsync();
            await LoadSigningStatusAsync();
            UpdateWorkflowSteps();
            DetermineRequiredAction();

            StatusMessage = $"Dernière vérification: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du statut CHIFA");
            HasError = true;
            LastError = $"Erreur de chargement: {ex.Message}";
            StatusMessage = "Erreur lors de la vérification";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadModeStatusAsync()
    {
        IntegrationMode = _modeProvider.CurrentMode.ToString();
        IsReadOnly = _modeProvider.IsReadOnly;

        ModeDescription = _modeProvider.CurrentMode switch
        {
            ChifaIntegrationMode.ReadOnly => "Lecture seule - Aucune écriture possible vers CHIFA-OFFICINE",
            ChifaIntegrationMode.Test => "Mode test - Les écritures utilisent une base de test",
            ChifaIntegrationMode.Production => "Mode production - Les écritures sont actives",
            _ => "Non configuré"
        };
    }

    private async Task LoadConnectionStatusAsync()
    {
        try
        {
            IsPostgresConnected = await _integrationService.IsChifaAvailableAsync();
            var health = await _integrationService.GetHealthStatusAsync();

            if (health.IsOnline && health.IsDatabaseConnected)
            {
                ConnectionStatus = "Connecté";
                ConnectionStatusColor = "#4CAF50";
                IsPostgresConnected = true;
            }
            else if (health.IsOnline)
            {
                ConnectionStatus = "Partiellement connecté";
                ConnectionStatusColor = "#FF9800";
                IsPostgresConnected = false;
            }
            else
            {
                ConnectionStatus = "Déconnecté";
                ConnectionStatusColor = "#F44336";
                IsPostgresConnected = false;
            }

            if (!string.IsNullOrEmpty(health.ErrorMessage))
            {
                LastError = health.ErrorMessage;
                HasError = true;
            }

            ChifaStatus = health.IsOnline ? "En ligne" : "Hors ligne";
            ChifaStatusColor = health.IsOnline ? "#4CAF50" : "#F44336";
            IsChifaAvailable = health.IsOnline;
        }
        catch (Exception ex)
        {
            ConnectionStatus = "Erreur de connexion";
            ConnectionStatusColor = "#F44336";
            IsPostgresConnected = false;
            ChifaStatus = "Indisponible";
            ChifaStatusColor = "#F44336";
            LastError = $"PostgreSQL indisponible: {ex.Message}";
            HasError = true;
            _logger.LogWarning(ex, "PostgreSQL connection check failed");
        }
    }

    private async Task LoadTokenStatusAsync()
    {
        try
        {
            IsTokenPresent = await _tokenService.IsTokenPresentAsync();
            var tokenInfo = await _tokenService.GetTokenInfoAsync();

            if (tokenInfo != null && tokenInfo.IsValid)
            {
                TokenStatus = $"Présent ({tokenInfo.Label})";
                TokenStatusColor = "#4CAF50";
                IsTokenPresent = true;
            }
            else if (IsTokenPresent)
            {
                TokenStatus = "Présent (expiry inconnue)";
                TokenStatusColor = "#FF9800";
            }
            else
            {
                TokenStatus = "Non détecté";
                TokenStatusColor = "#F44336";
                IsTokenPresent = false;
            }
        }
        catch (Exception ex)
        {
            TokenStatus = "Erreur de détection";
            TokenStatusColor = "#F44336";
            IsTokenPresent = false;
            _logger.LogWarning(ex, "Token check failed");
        }
    }

    private async Task LoadSigningStatusAsync()
    {
        try
        {
            var status = await _signingService.GetSigningStatusAsync("_global");
            SigningStatus = status switch
            {
                ChifaSigningStatus.NotSigned => "Non signé",
                ChifaSigningStatus.SigningRequired => "Signature requise",
                ChifaSigningStatus.SigningInProgress => "Signature en cours",
                ChifaSigningStatus.Signed => "Signé",
                ChifaSigningStatus.SigningFailed => "Échec de signature",
                ChifaSigningStatus.TokenNotPresent => "Token non présent",
                _ => "Inconnu"
            };
            SigningStatusColor = status switch
            {
                ChifaSigningStatus.Signed => "#4CAF50",
                ChifaSigningStatus.SigningInProgress => "#FF9800",
                ChifaSigningStatus.SigningFailed => "#F44336",
                ChifaSigningStatus.TokenNotPresent => "#F44336",
                _ => "#757575"
            };
        }
        catch (Exception ex)
        {
            SigningStatus = "Erreur";
            SigningStatusColor = "#F44336";
            _logger.LogWarning(ex, "Signing status check failed");
        }
    }

    private void UpdateWorkflowSteps()
    {
        // Step 1: Vente (always ready in BM Pharma)
        WorkflowStep1Status = IsPostgresConnected ? "Prêt" : "En attente connexion";
        WorkflowStep1Color = IsPostgresConnected ? "#4CAF50" : "#757575";

        // Step 2: Préparation CHIFA
        WorkflowStep2Status = PreparedInvoiceCount > 0 ? $"{PreparedInvoiceCount} facture(s) prête(s)" : "En attente";
        WorkflowStep2Color = PreparedInvoiceCount > 0 ? "#4CAF50" : "#757575";

        // Step 3: Validation
        WorkflowStep3Status = IsPostgresConnected ? "Prêt" : "Non disponible";
        WorkflowStep3Color = IsPostgresConnected ? "#4CAF50" : "#F44336";

        // Step 4: Synchronisation
        WorkflowStep4Status = SynchronizedInvoiceCount > 0 ? $"{SynchronizedInvoiceCount} synchronisée(s)" : "En attente";
        WorkflowStep4Color = SynchronizedInvoiceCount > 0 ? "#4CAF50" : "#757575";

        // Step 5: Action pharmacien
        WorkflowStep5Status = HasRequiredAction ? "Action requise" : "En attente";
        WorkflowStep5Color = HasRequiredAction ? "#FF9800" : "#757575";

        // Step 6: Signature CHIFA
        WorkflowStep6Status = SigningStatus;
        WorkflowStep6Color = SigningStatusColor;

        // Step 7: Clôture bordereau
        WorkflowStep7Status = BordereauPreparedCount > 0 ? $"{BordereauPreparedCount} bordereau(x)" : "En attente";
        WorkflowStep7Color = BordereauPreparedCount > 0 ? "#4CAF50" : "#757575";

        // Step 8: Transmission CNAS
        WorkflowStep8Status = CnasTransmissionStatus;
        WorkflowStep8Color = CnasTransmissionColor;
    }

    private void DetermineRequiredAction()
    {
        if (IsReadOnly)
        {
            HasRequiredAction = false;
            RequiredActionTitle = "";
            RequiredActionDescription = "";
            RequiredActionApplication = "";
            RequiredActionBmPharmaWaits = "";
            return;
        }

        if (!IsTokenPresent && IsPostgresConnected)
        {
            HasRequiredAction = true;
            RequiredActionTitle = "Token professionnel requis";
            RequiredActionDescription = "Pour signer les bordereaux, le pharmacien doit insérer son token PKCS#11 professionnel.";
            RequiredActionApplication = "CHIFA-OFFICINE";
            RequiredActionBmPharmaWaits = "BM Pharma attend la détection du token pour permettre la signature.";
            return;
        }

        if (IsTokenPresent && SigningStatus == "Signature requise")
        {
            HasRequiredAction = true;
            RequiredActionTitle = "Signature du bordereau requise";
            RequiredActionDescription = "Un ou plusieurs bordereaux nécessitent une signature du pharmacien.";
            RequiredActionApplication = "CHIFA-OFFICINE";
            RequiredActionBmPharmaWaits = "BM Pharma attend que la signature soit effectuée dans CHIFA-OFFICINE.";
            return;
        }

        HasRequiredAction = false;
    }
}
