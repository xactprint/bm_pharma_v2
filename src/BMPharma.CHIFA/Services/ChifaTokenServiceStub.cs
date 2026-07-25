using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaTokenServiceStub : Interfaces.IChifaTokenService
{
    private readonly ILogger<ChifaTokenServiceStub> _logger;

    public ChifaTokenServiceStub(ILogger<ChifaTokenServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsTokenPresentAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA token check not yet implemented");
        return Task.FromResult(false);
    }

    public Task<Interfaces.TokenInfo?> GetTokenInfoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Interfaces.TokenInfo?>(null);
    }
}
