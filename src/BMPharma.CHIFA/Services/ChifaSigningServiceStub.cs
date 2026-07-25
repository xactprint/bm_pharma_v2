using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;

namespace BMPharma.CHIFA.Services;

public class ChifaSigningServiceStub : IChifaSigningService
{
    private readonly ILogger<ChifaSigningServiceStub> _logger;

    public ChifaSigningServiceStub(ILogger<ChifaSigningServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<ChifaSigningStatus> GetSigningStatusAsync(string bordereauNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Signing status check for bordereau {NumBord} — delegated to CHIFA", bordereauNumber);
        return Task.FromResult(ChifaSigningStatus.NotSigned);
    }

    public Task<bool> IsTokenAvailableAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Token detection is not implemented in BM Pharma. Use CHIFA-OFFICINE.");
        return Task.FromResult(false);
    }
}
