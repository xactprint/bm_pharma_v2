using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BMPharma.Domain.Enums;

namespace BMPharma.UI.ViewModels;

public partial class ChifaDashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private string _integrationMode = "ReadOnly";

    [ObservableProperty]
    private DateTime? _lastSyncDate;

    [ObservableProperty]
    private int _preparedInvoiceCount;

    [ObservableProperty]
    private int _writtenInvoiceCount;

    [ObservableProperty]
    private int _bordereauCount;

    [ObservableProperty]
    private string _chifaStatus = "Unknown";

    public ChifaDashboardViewModel()
    {
        Title = "CHIFA Integration Dashboard";
        ConnectionStatus = "Disconnected (default)";
        IntegrationMode = "ReadOnly";
        ChifaStatus = "Not configured";
    }

    [RelayCommand]
    private void RefreshStatus()
    {
        // Will be wired to real services in Phase 3
    }
}
