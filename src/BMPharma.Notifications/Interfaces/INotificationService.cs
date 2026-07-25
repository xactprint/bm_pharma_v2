namespace BMPharma.Notifications.Interfaces;

public interface INotificationService
{
    Task ShowInfoAsync(string title, string message, CancellationToken cancellationToken = default);
    Task ShowWarningAsync(string title, string message, CancellationToken cancellationToken = default);
    Task ShowErrorAsync(string title, string message, CancellationToken cancellationToken = default);
    Task<bool> ShowConfirmationAsync(string title, string message, CancellationToken cancellationToken = default);
}
