using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class InvoiceSynchronizer
{
    private readonly IChifaInvoiceService _invoice;
    private readonly IChifaIntegrationService _integration;
    private readonly ILogger<InvoiceSynchronizer> _logger;

    public InvoiceSynchronizer(
        IChifaInvoiceService invoice,
        IChifaIntegrationService integration,
        ILogger<InvoiceSynchronizer> logger)
    {
        _invoice = invoice;
        _integration = integration;
        _logger = logger;
    }

    public async Task<ChifaSyncSummary> SynchronizeAsync(
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var summary = new ChifaSyncSummary { Timestamp = DateTime.UtcNow };

        try
        {
            var available = await _integration.IsChifaAvailableAsync(ct).ConfigureAwait(false);
            if (!available)
            {
                summary.Errors.Add("CHIFA is not available");
                summary.DurationMs = sw.ElapsedMilliseconds;
                return summary;
            }

            summary.DurationMs = sw.ElapsedMilliseconds;
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice synchronization failed");
            summary.Errors.Add(ex.Message);
            summary.DurationMs = sw.ElapsedMilliseconds;
            return summary;
        }
    }
}
