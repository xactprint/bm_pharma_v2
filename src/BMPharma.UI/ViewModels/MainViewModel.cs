using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BMPharma.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _currentView = "Home";

    [ObservableProperty]
    private string _userName = "Admin";

    [ObservableProperty]
    private string _chifaStatus = "Disconnected";

    public MainViewModel()
    {
        Title = "BM Pharma v2";
    }

    [RelayCommand]
    private void NavigateTo(string viewName)
    {
        CurrentView = viewName;
    }
}
