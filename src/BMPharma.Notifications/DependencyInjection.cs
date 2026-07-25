using Microsoft.Extensions.DependencyInjection;
using BMPharma.Notifications.Interfaces;
using BMPharma.Notifications.Services;

namespace BMPharma.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationServiceStub>();
        return services;
    }
}
