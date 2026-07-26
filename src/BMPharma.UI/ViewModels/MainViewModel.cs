using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BMPharma.CHIFA.Interfaces;

namespace BMPharma.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ChifaIntegrationModeProvider? _modeProvider;

    [ObservableProperty]
    private string _currentView = "Home";

    [ObservableProperty]
    private string _userName = "Admin";

    [ObservableProperty]
    private string _chifaStatus = "Déconnecté";

    [ObservableProperty]
    private string _chifaStatusColor = "#F44336";

    public MainViewModel()
    {
        Title = "BM Pharma v2";
    }

    public MainViewModel(ChifaIntegrationModeProvider modeProvider)
    {
        _modeProvider = modeProvider;
        Title = "BM Pharma v2";
        UpdateChifaStatus();
    }

    [RelayCommand]
    private void NavigateTo(string viewName)
    {
        CurrentView = viewName;
    }

    private void UpdateChifaStatus()
    {
        if (_modeProvider == null)
        {
            ChifaStatus = "Non configuré";
            ChifaStatusColor = "#757575";
            return;
        }

        ChifaStatus = _modeProvider.CurrentMode switch
        {
            Domain.Enums.ChifaIntegrationMode.ReadOnly => "CHIFA: Lecture seule",
            Domain.Enums.ChifaIntegrationMode.Test => "CHIFA: Mode test",
            Domain.Enums.ChifaIntegrationMode.Production => "CHIFA: Production",
            _ => "CHIFA: Non configuré"
        };

        ChifaStatusColor = _modeProvider.CurrentMode switch
        {
            Domain.Enums.ChifaIntegrationMode.ReadOnly => "#FF9800",
            Domain.Enums.ChifaIntegrationMode.Test => "#2196F3",
            Domain.Enums.ChifaIntegrationMode.Production => "#4CAF50",
            _ => "#757575"
        };
    }
}
