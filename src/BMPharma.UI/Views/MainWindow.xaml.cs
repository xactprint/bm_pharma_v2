using System.Windows;
using System.Windows.Controls;

namespace BMPharma.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            // Navigation will be wired up with ViewModels in Phase 2
            MainContent.Content = new TextBlock
            {
                Text = $"{tag} — Coming soon",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray
            };
            StatusText.Text = $"Viewing: {tag}";
        }
    }
}
