using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class BordereauSynchronizer
{
    private readonly IChifaBordereauService _bordereau;
    private readonly IChifaIntegrationService _integration;
    private readonly IBordereauStatusService _bordereauStatus;
    private readonly ILogger<BordereauSynchronizer> _logger;

    public BordereauSynchronizer(
        IChifaBordereauService bordereau,
        IChifaIntegrationService integration,
        IBordereauStatusService bordereauStatus,
        ILogger<BordereauSynchronizer> logger)
    {
        _bordereau = bordereau;
        _integration = integration;
        _bordereauStatus = bordereauStatus;
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
            _logger.LogError(ex, "Bordereau synchronization failed");
            summary.Errors.Add(ex.Message);
            summary.DurationMs = sw.ElapsedMilliseconds;
            return summary;
        }
    }
}
