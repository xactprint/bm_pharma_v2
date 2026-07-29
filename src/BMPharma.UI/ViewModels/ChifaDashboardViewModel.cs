using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace BMPharma.UI.ViewModels;

public partial class ChifaDashboardViewModel : ViewModelBase
{
    private readonly IChifaIntegrationFacade _facade;
    private readonly ChifaIntegrationModeProvider _modeProvider;
    private readonly ILogger<ChifaDashboardViewModel> _logger;
    private readonly Timer _refreshTimer;

    private const int AutoRefreshIntervalMs = 30_000;

    public ChifaDashboardViewModel(
        IChifaIntegrationFacade facade,
        ChifaIntegrationModeProvider modeProvider,
        ILogger<ChifaDashboardViewModel> logger)
    {
        _facade = facade;
        _modeProvider = modeProvider;
        _logger = logger;

        Title = "Tableau de bord CHIFA";
        IntegrationMode = _modeProvider.CurrentMode.ToString();
        IsReadOnly = _modeProvider.IsReadOnly;

        _refreshTimer = new Timer(AutoRefreshIntervalMs);
        _refreshTimer.Elapsed += async (_, _) => await LoadStatusAsync();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();

        _ = LoadStatusAsync();
    }

    [ObservableProperty] private string _connectionStatus = "Vérification...";
    [ObservableProperty] private string _connectionStatusColor = "#FF9800";
    [ObservableProperty] private bool _isPostgresConnected;

    [ObservableProperty] private string _integrationMode = "ReadOnly";
    [ObservableProperty] private bool _isReadOnly = true;
    [ObservableProperty] private string _modeDescription = "";

    [ObservableProperty] private string _chifaStatus = "Vérification...";
    [ObservableProperty] private string _chifaStatusColor = "#FF9800";
    [ObservableProperty] private bool _isChifaAvailable;

    [ObservableProperty] private string _tokenStatus = "Vérification...";
    [ObservableProperty] private string _tokenStatusColor = "#FF9800";
    [ObservableProperty] private bool _isTokenPresent;

    [ObservableProperty] private string _signingStatus = "Non vérifié";
    [ObservableProperty] private string _signingStatusColor = "#757575";

    [ObservableProperty] private int _preparedInvoiceCount;
    [ObservableProperty] private int _synchronizedInvoiceCount;
    [ObservableProperty] private int _bordereauPreparedCount;
    [ObservableProperty] private string _nextBordereauNumber = "-";

    [ObservableProperty] private string _cnasTransmissionStatus = "En attente";
    [ObservableProperty] private string _cnasTransmissionColor = "#757575";

    [ObservableProperty] private string _lastOperation = "Aucune";
    [ObservableProperty] private string _lastOperationTime = "-";

    [ObservableProperty] private string _lastError = "";
    [ObservableProperty] private bool _hasError;

    [ObservableProperty] private string _statusMessage = "Chargement en cours...";

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

    [ObservableProperty] private bool _hasRequiredAction;
    [ObservableProperty] private string _requiredActionTitle = "";
    [ObservableProperty] private string _requiredActionDescription = "";
    [ObservableProperty] private string _requiredActionApplication = "";
    [ObservableProperty] private string _requiredActionBmPharmaWaits = "";

    [ObservableProperty] private string _circuitBreakerState = "Inconnu";
    [ObservableProperty] private string _circuitBreakerColor = "#757575";
    [ObservableProperty] private int _metricsTotal;
    [ObservableProperty] private int _metricsSuccess;
    [ObservableProperty] private int _metricsFailed;
    [ObservableProperty] private string _metricsAvgMs = "-";
    [ObservableProperty] private string _correlationId = "-";
    [ObservableProperty] private string _lastSyncResult = "-";
    [ObservableProperty] private string _lastSyncTimeDisplay = "-";

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

            var overviewResult = await _facade.GetDashboardOverviewAsync();

            if (overviewResult.IsSuccess && overviewResult.Data != null)
            {
                var data = overviewResult.Data;

                ConnectionStatus = data.Status.Technical switch
                {
                    TechnicalStatus.Connected => "Connecté",
                    TechnicalStatus.Degraded => "Dégradé",
                    TechnicalStatus.Disconnected => "Déconnecté",
                    _ => "Inconnu"
                };
                ConnectionStatusColor = data.Status.Technical switch
                {
                    TechnicalStatus.Connected => "#4CAF50",
                    TechnicalStatus.Degraded => "#FF9800",
                    _ => "#F44336"
                };
                IsPostgresConnected = data.Status.Technical == TechnicalStatus.Connected;

                ChifaStatus = data.Status.IsReady ? "Prêt" : data.Status.Technical == TechnicalStatus.Connected ? "En ligne" : "Hors ligne";
                ChifaStatusColor = data.Status.IsReady ? "#4CAF50" : data.Status.Technical == TechnicalStatus.Connected ? "#FF9800" : "#F44336";
                IsChifaAvailable = data.Status.Technical != TechnicalStatus.Disconnected;

                TokenStatus = data.Status.Business != BusinessStatus.Unknown ? "Présent" : "Vérification...";
                TokenStatusColor = data.Status.Business != BusinessStatus.Unknown ? "#4CAF50" : "#FF9800";
                IsTokenPresent = data.Status.Business != BusinessStatus.Unknown;

                PreparedInvoiceCount = data.TotalInvoicesToday;
                SynchronizedInvoiceCount = data.LastSync?.InvoicesUpdated ?? 0;
                BordereauPreparedCount = data.PendingBordereaux;
                NextBordereauNumber = data.LastSync?.BordereauxFound > 0 ? $"{data.LastSync.BordereauxFound} trouvé(s)" : "-";
                CnasTransmissionStatus = data.Status.Visibility == VisibilityStatus.NotVisible ? "Prêt" : "Transmis";
                CnasTransmissionColor = data.Status.Visibility == VisibilityStatus.NotVisible ? "#4CAF50" : "#757575";

                LastOperation = data.LastOperation ?? "Aucune";
                LastOperationTime = data.Timestamp.ToLocalTime().ToString("HH:mm:ss");

                CircuitBreakerState = data.CircuitBreakerState ?? "Inconnu";
                CircuitBreakerColor = data.CircuitBreakerState switch
                {
                    "Closed" => "#4CAF50",
                    "HalfOpen" => "#FF9800",
                    "Open" => "#F44336",
                    _ => "#757575"
                };
                MetricsTotal = data.MetricsTotal;
                MetricsSuccess = data.MetricsSuccess;
                MetricsFailed = data.MetricsFailed;
                MetricsAvgMs = data.MetricsAvgMs > 0 ? $"{data.MetricsAvgMs} ms" : "-";
                CorrelationId = data.CorrelationId ?? "-";
                LastSyncResult = data.LastSyncResult ?? "-";
                LastSyncTimeDisplay = data.LastSyncTime?.ToLocalTime().ToString("HH:mm:ss") ?? "-";

                if (!string.IsNullOrEmpty(data.Status.ErrorMessage))
                {
                    LastError = data.Status.ErrorMessage;
                    HasError = true;
                }
            }
            else
            {
                ConnectionStatus = "Indisponible";
                ConnectionStatusColor = "#F44336";
                IsPostgresConnected = false;
                ChifaStatus = "Indisponible";
                ChifaStatusColor = "#F44336";

                if (!string.IsNullOrEmpty(overviewResult.ErrorMessage))
                {
                    LastError = overviewResult.ErrorMessage.ToUserMessage();
                    HasError = true;
                }
            }

            UpdateWorkflowSteps();
            DetermineRequiredAction();

            StatusMessage = $"Dernière vérification: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du statut CHIFA");
            HasError = true;
            LastError = $"Erreur de chargement: {ex.ToUserMessage()}";
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

    private void UpdateWorkflowSteps()
    {
        WorkflowStep1Status = IsPostgresConnected ? "Prêt" : "En attente connexion";
        WorkflowStep1Color = IsPostgresConnected ? "#4CAF50" : "#757575";

        WorkflowStep2Status = PreparedInvoiceCount > 0 ? $"{PreparedInvoiceCount} facture(s) aujourd'hui" : "En attente";
        WorkflowStep2Color = PreparedInvoiceCount > 0 ? "#4CAF50" : "#757575";

        WorkflowStep3Status = IsPostgresConnected ? "Prêt" : "Non disponible";
        WorkflowStep3Color = IsPostgresConnected ? "#4CAF50" : "#F44336";

        WorkflowStep4Status = SynchronizedInvoiceCount > 0 ? $"{SynchronizedInvoiceCount} synchronisée(s)" : "En attente";
        WorkflowStep4Color = SynchronizedInvoiceCount > 0 ? "#4CAF50" : "#757575";

        WorkflowStep5Status = HasRequiredAction ? "Action requise" : "En attente";
        WorkflowStep5Color = HasRequiredAction ? "#FF9800" : "#757575";

        WorkflowStep6Status = SigningStatus;
        WorkflowStep6Color = SigningStatusColor;

        WorkflowStep7Status = BordereauPreparedCount > 0 ? $"{BordereauPreparedCount} en attente" : "En attente";
        WorkflowStep7Color = BordereauPreparedCount > 0 ? "#4CAF50" : "#757575";

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
