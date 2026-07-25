using CommunityToolkit.Mvvm.ComponentModel;

namespace BMPharma.UI.ViewModels;

public partial class ChifaInvoicePreparationViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _invoiceNumber = string.Empty;

    [ObservableProperty]
    private string _insuranceNumber = string.Empty;

    [ObservableProperty]
    private int _centreCode = 11600;

    [ObservableProperty]
    private DateTime _careDate = DateTime.Today;

    [ObservableProperty]
    private string _validationStatus = string.Empty;

    [ObservableProperty]
    private string _lastError = string.Empty;

    public ChifaInvoicePreparationViewModel()
    {
        Title = "CHIFA Invoice Preparation";
    }
}
