using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using BMPharma.Persistence.SQLite.Contexts;
using Microsoft.EntityFrameworkCore;
using BMPharma.CHIFA;
using BMPharma.Notifications;
using BMPharma.UI.ViewModels;
using BMPharma.UI.Views;

namespace BMPharma.UI;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        System.IO.Path.Combine("logs", "bmpharma-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30);
            })
            .ConfigureServices((context, services) =>
            {
                // EF Core SQLite
                services.AddDbContext<BmPharmaDbContext>(options =>
                    options.UseSqlite($"Data Source=bmpharma.db"));

                // CHIFA Integration (mode-based switching)
                services.AddChifaIntegration(context.Configuration);

                // Notifications (stubs)
                services.AddNotifications();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<ChifaDashboardViewModel>();
                services.AddTransient<ChifaInvoicePreparationViewModel>();
            })
            .Build();

        ServiceProvider = _host.Services;
        await _host.StartAsync();

        // Initialize database
        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BmPharmaDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Create and show main window with DI
        var mainWindow = new MainWindow();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Une erreur inattendue s'est produite:\n\n{e.Exception.Message}",
            "BM Pharma - Erreur",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
