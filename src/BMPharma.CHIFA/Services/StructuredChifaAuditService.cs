using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;

namespace BMPharma.CHIFA.Services;

public class StructuredChifaAuditService : IChifaAuditService
{
    private readonly ILogger<StructuredChifaAuditService> _logger;
    private readonly ConcurrentBag<StructuredAuditEntry> _entries = new();

    public StructuredChifaAuditService(ILogger<StructuredChifaAuditService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<StructuredAuditEntry> Entries => _entries.ToArray().OrderBy(e => e.Timestamp).ToList();

    public Task LogOperationAsync(string operation, string? entityType, string? entityKey,
        bool success, long durationMs, string? errorMessage = null, string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return LogOperationAsync(operation, entityType, entityKey, string.Empty, success, durationMs, errorMessage, userId, cancellationToken);
    }

    public Task LogOperationAsync(string operation, string? entityType, string? entityKey,
        string details, bool success, long durationMs, string? errorMessage = null,
        string? userId = null, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        var entry = new StructuredAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId,
            UserId = userId ?? "system",
            Operation = operation,
            EntityType = entityType,
            EntityKey = entityKey,
            Source = "BM_PHARMA",
            Destination = DetermineDestination(operation),
            Result = success ? "SUCCESS" : "ERROR",
            Details = details,
            Error = errorMessage,
            DurationMs = durationMs
        };

        _entries.Add(entry);

        if (success)
        {
            _logger.LogInformation(
                "[CHIFA-AUDIT] CorId={CorrelationId} Op={Operation} Entity={EntityType}/{EntityKey} " +
                "Src={Source} Dst={Destination} Result={Result} Duration={Duration}ms User={User} Details={Details}",
                correlationId, operation, entityType, entityKey, entry.Source, entry.Destination,
                entry.Result, durationMs, entry.UserId, details ?? "none");
        }
        else
        {
            _logger.LogWarning(
                "[CHIFA-AUDIT] CorId={CorrelationId} Op={Operation} Entity={EntityType}/{EntityKey} " +
                "Src={Source} Dst={Destination} Result={Result} Duration={Duration}ms Error={Error} User={User}",
                correlationId, operation, entityType, entityKey, entry.Source, entry.Destination,
                entry.Result, durationMs, errorMessage ?? "none", entry.UserId);
        }

        return Task.CompletedTask;
    }

    public void Clear() => _entries.Clear();

    private static string DetermineDestination(string operation)
    {
        return operation switch
        {
            var op when op.Contains("CHIFA") => "CHIFA_POSTGRESQL",
            var op when op.Contains("SQLITE") => "BM_SQLITE",
            _ => "BM_PHARMA"
        };
    }
}

public class StructuredAuditEntry
{
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityKey { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Error { get; set; }
    public long DurationMs { get; set; }
}
