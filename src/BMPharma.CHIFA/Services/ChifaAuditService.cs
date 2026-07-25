using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaAuditService : Interfaces.IChifaAuditService
{
    private readonly ILogger<ChifaAuditService> _logger;

    public ChifaAuditService(ILogger<ChifaAuditService> logger)
    {
        _logger = logger;
    }

    public Task LogOperationAsync(string operation, string? entityType, string? entityKey,
        bool success, long durationMs, string? errorMessage = null, string? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[CHIFA-AUDIT] Op={Operation} Entity={EntityType} Key={EntityKey} Success={Success} Duration={Duration}ms Error={Error} User={User} Correlation={Correlation}",
            operation, entityType, entityKey, success, durationMs, errorMessage ?? "none", userId ?? "system", Guid.NewGuid().ToString("N")[..8]);
        return Task.CompletedTask;
    }

    public Task LogOperationAsync(string operation, string? entityType, string? entityKey,
        string details, bool success, long durationMs, string? errorMessage = null,
        string? userId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[CHIFA-AUDIT] Op={Operation} Entity={EntityType} Key={EntityKey} Details={Details} Success={Success} Duration={Duration}ms Error={Error} User={User}",
            operation, entityType, entityKey, details, success, durationMs, errorMessage ?? "none", userId ?? "system");
        return Task.CompletedTask;
    }
}
