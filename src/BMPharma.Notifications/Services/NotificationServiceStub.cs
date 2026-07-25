using Microsoft.Extensions.Logging;
using BMPharma.Notifications.Interfaces;

namespace BMPharma.Notifications.Services;

public class NotificationServiceStub : INotificationService
{
    private readonly ILogger<NotificationServiceStub> _logger;

    public NotificationServiceStub(ILogger<NotificationServiceStub> logger)
    {
        _logger = logger;
    }

    public Task ShowInfoAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{Title}] {Message}", title, message);
        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("[{Title}] {Message}", title, message);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogError("[{Title}] {Message}", title, message);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("[{Title}] {Message} (auto-confirmed in stub)", title, message);
        return Task.FromResult(true);
    }
}
