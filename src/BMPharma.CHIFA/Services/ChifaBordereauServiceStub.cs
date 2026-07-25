using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaBordereauServiceStub : Interfaces.IChifaBordereauService
{
    private readonly ILogger<ChifaBordereauServiceStub> _logger;

    public ChifaBordereauServiceStub(ILogger<ChifaBordereauServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<Interfaces.ChifaBordereauResult> CreateBordereauAsync(Interfaces.ChifaBordereauRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA bordereau creation not yet implemented");
        return Task.FromResult(new Interfaces.ChifaBordereauResult
        {
            Success = false,
            ErrorMessage = "CHIFA integration not yet implemented"
        });
    }

    public Task<Interfaces.ChifaBordereauResult> SignBordereauAsync(string numBord, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA signing not yet implemented");
        return Task.FromResult(new Interfaces.ChifaBordereauResult
        {
            Success = false,
            ErrorMessage = "CHIFA signing not yet implemented"
        });
    }

    public Task<Interfaces.ChifaBordereauResult> CloseBordereauAsync(string numBord, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA closure not yet implemented");
        return Task.FromResult(new Interfaces.ChifaBordereauResult
        {
            Success = false,
            ErrorMessage = "CHIFA closure not yet implemented"
        });
    }

    public Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA bordereau number generation not yet implemented");
        return Task.FromResult("000001");
    }
}
