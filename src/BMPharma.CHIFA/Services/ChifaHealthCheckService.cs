using BMPharma.CHIFA.Interfaces;
using BMPharma.Persistence.PostgreSQL.Contexts;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaHealthCheckService
{
    private readonly IChifaIntegrationService _integration;
    private readonly IChifaTokenService _token;
    private readonly IChifaSigningService _signing;
    private readonly ChifaPostgreSqlContext _readContext;
    private readonly ChifaWriteDbContext _writeContext;
    private readonly ILogger<ChifaHealthCheckService> _logger;

    public ChifaHealthCheckService(
        IChifaIntegrationService integration,
        IChifaTokenService token,
        IChifaSigningService signing,
        ChifaPostgreSqlContext readContext,
        ChifaWriteDbContext writeContext,
        ILogger<ChifaHealthCheckService> logger)
    {
        _integration = integration;
        _token = token;
        _signing = signing;
        _readContext = readContext;
        _writeContext = writeContext;
        _logger = logger;
    }

    public async Task<ChifaHealthStatus> CheckAllAsync(
        CancellationToken ct = default)
    {
        try
        {
            return await _integration.GetHealthStatusAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return new ChifaHealthStatus
            {
                IsOnline = false,
                IsDatabaseConnected = false,
                IsTokenPresent = false,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };
        }
    }
}
