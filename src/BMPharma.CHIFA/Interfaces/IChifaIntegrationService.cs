namespace BMPharma.CHIFA.Interfaces;

public interface IChifaIntegrationService
{
    Task<bool> IsChifaAvailableAsync(CancellationToken cancellationToken = default);
    Task<ChifaHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

public class ChifaHealthStatus
{
    public bool IsOnline { get; set; }
    public bool IsDatabaseConnected { get; set; }
    public bool IsTokenPresent { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
