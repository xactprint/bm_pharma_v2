using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class ChifaInvoiceServiceStub : Interfaces.IChifaInvoiceService
{
    private readonly ILogger<ChifaInvoiceServiceStub> _logger;

    public ChifaInvoiceServiceStub(ILogger<ChifaInvoiceServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<Interfaces.ChifaInvoiceResult> CreateInvoiceAsync(Interfaces.ChifaInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("CHIFA invoice creation not yet implemented");
        return Task.FromResult(new Interfaces.ChifaInvoiceResult
        {
            Success = false,
            ErrorMessage = "CHIFA integration not yet implemented"
        });
    }

    public Task<bool> InvoiceExistsInChifaAsync(string numFact, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
