using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;

namespace BMPharma.CHIFA.Services;

public class StatusEngine
{
    private readonly IChifaIntegrationService _integration;
    private readonly IChifaTokenService _token;
    private readonly IChifaSigningService _signing;
    private readonly IChifaInvoiceService _invoice;
    private readonly IChifaBordereauService _bordereau;
    private readonly ChifaCircuitBreaker _circuitBreaker;
    private readonly ILogger<StatusEngine> _logger;

    public StatusEngine(
        IChifaIntegrationService integration,
        IChifaTokenService token,
        IChifaSigningService signing,
        IChifaInvoiceService invoice,
        IChifaBordereauService bordereau,
        ChifaCircuitBreaker circuitBreaker,
        ILogger<StatusEngine> logger)
    {
        _integration = integration;
        _token = token;
        _signing = signing;
        _invoice = invoice;
        _bordereau = bordereau;
        _circuitBreaker = circuitBreaker;
        _logger = logger;
    }

    public async Task<ChifaStatusSnapshot> EvaluateAsync(
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var snapshot = new ChifaStatusSnapshot { Timestamp = DateTime.UtcNow };

        try
        {
            if (_circuitBreaker.IsOpen("chifa"))
            {
                snapshot.Technical = TechnicalStatus.Disconnected;
                snapshot.ErrorMessage = "Circuit breaker is open";
                snapshot.DurationMs = sw.ElapsedMilliseconds;
                return snapshot;
            }

            var health = await _integration.GetHealthStatusAsync(ct).ConfigureAwait(false);

            if (health.IsOnline && health.IsDatabaseConnected)
                snapshot.Technical = TechnicalStatus.Connected;
            else if (health.IsOnline || health.IsDatabaseConnected)
                snapshot.Technical = TechnicalStatus.Degraded;
            else
                snapshot.Technical = TechnicalStatus.Disconnected;

            snapshot.ErrorMessage = health.ErrorMessage;
            _circuitBreaker.RecordSuccess("chifa");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "StatusEngine evaluation failed");
            snapshot.Technical = TechnicalStatus.Disconnected;
            snapshot.ErrorMessage = ex.Message;
            _circuitBreaker.RecordFailure("chifa");
        }

        snapshot.DurationMs = sw.ElapsedMilliseconds;
        return snapshot;
    }
}
