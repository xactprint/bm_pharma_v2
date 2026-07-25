using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaIntegrationServiceStub : Interfaces.IChifaIntegrationService
{
    private readonly ILogger<ChifaIntegrationServiceStub> _logger;

    public ChifaIntegrationServiceStub(ILogger<ChifaIntegrationServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsChifaAvailableAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA integration not yet implemented — returning false");
        return Task.FromResult(false);
    }

    public Task<Interfaces.ChifaHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA health check not yet implemented");
        return Task.FromResult(new Interfaces.ChifaHealthStatus
        {
            IsOnline = false,
            IsDatabaseConnected = false,
            IsTokenPresent = false,
            ErrorMessage = "CHIFA integration not yet implemented"
        });
    }
}
