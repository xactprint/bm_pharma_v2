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

namespace BMPharma.UI;

public partial class App : System.Windows.Application
{
    private IHost? _host;

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

                // CHIFA Integration (stubs)
                services.AddChifaIntegration();

                // Notifications (stubs)
                services.AddNotifications();

                // ViewModels will be registered here
            })
            .Build();

        await _host.StartAsync();

        // Initialize database
        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BmPharmaDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
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
            $"An unexpected error occurred:\n\n{e.Exception.Message}",
            "BM Pharma - Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
