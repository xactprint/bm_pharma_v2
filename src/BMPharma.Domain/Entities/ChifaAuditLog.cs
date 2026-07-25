using BMPharma.Domain.Common;

namespace BMPharma.Domain.Entities;

public class ChifaAuditLog : BaseEntity
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityKey { get; set; }
    public string? Details { get; set; }
    public bool Success { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
}
