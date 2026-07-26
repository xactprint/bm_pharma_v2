using System.Windows;
using System.Windows.Controls;
using BMPharma.UI.ViewModels;

namespace BMPharma.UI.Views;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Func<UserControl>> _viewFactories = new();
    private readonly Dictionary<string, UserControl> _viewCache = new();

    public MainWindow()
    {
        InitializeComponent();
        SetupViewFactories();
    }

    private void SetupViewFactories()
    {
        _viewFactories["Home"] = () => CreateHomeView();
        _viewFactories["ChifaDashboard"] = () => CreateChifaView<ChifaDashboardView>();
        _viewFactories["ChifaInvoicePreparation"] = () => CreateChifaView<ChifaInvoicePreparationView>();
        _viewFactories["ChifaBordereauStatus"] = () => CreateChifaView<ChifaBordereauStatusView>();
    }

    private UserControl CreateHomeView()
    {
        return new UserControl
        {
            Content = new TextBlock
            {
                Text = "Bienvenue dans BM Pharma v2\n\nUtilisez le menu de navigation pour accéder aux fonctionnalités.",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray
            }
        };
    }

    private UserControl CreateChifaView<T>() where T : UserControl, new()
    {
        var view = new T();
        var viewModel = App.ServiceProvider?.GetService(typeof(ChifaDashboardViewModel));
        if (viewModel != null)
        {
            view.DataContext = viewModel;
        }
        return view;
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            try
            {
                UserControl view;
                if (_viewFactories.TryGetValue(tag, out var factory))
                {
                    view = factory();
                }
                else
                {
                    view = new UserControl
                    {
                        Content = new TextBlock
                        {
                            Text = $"{tag} — Bientôt disponible",
                            FontSize = 24,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Foreground = System.Windows.Media.Brushes.Gray
                        }
                    };
                }

                MainContent.Content = view;
                StatusText.Text = $"Navigation: {tag}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur de navigation: {ex.Message}",
                    "BM Pharma - Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
