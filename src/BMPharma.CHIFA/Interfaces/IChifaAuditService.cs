namespace BMPharma.CHIFA.Interfaces;

public interface IChifaAuditService
{
    Task LogOperationAsync(string operation, string? entityType, string? entityKey, bool success, long durationMs, string? errorMessage = null, string? userId = null, CancellationToken cancellationToken = default);
    Task LogOperationAsync(string operation, string? entityType, string? entityKey, string details, bool success, long durationMs, string? errorMessage = null, string? userId = null, CancellationToken cancellationToken = default);
}
