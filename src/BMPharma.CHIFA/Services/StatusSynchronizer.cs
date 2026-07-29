using System.Diagnostics;
using BMPharma.CHIFA.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BMPharma.Persistence.PostgreSQL.Contexts;

namespace BMPharma.CHIFA.Services;

public class StatusSynchronizer
{
    private readonly ChifaPostgreSqlContext _readContext;
    private readonly IChifaIntegrationService _integration;
    private readonly ILogger<StatusSynchronizer> _logger;

    public StatusSynchronizer(
        ChifaPostgreSqlContext readContext,
        IChifaIntegrationService integration,
        ILogger<StatusSynchronizer> logger)
    {
        _readContext = readContext;
        _integration = integration;
        _logger = logger;
    }

    public async Task<ChifaInvoiceStatusSnapshot?> LoadInvoiceAsync(
        string numFact, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var facture = await _readContext.ChifaFactures
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.NumFact == numFact, ct)
                .ConfigureAwait(false);

            if (facture == null) return null;

            return new ChifaInvoiceStatusSnapshot
            {
                NumFact = facture.NumFact,
                Etat = facture.Etat,
                NumBord = facture.NumBord,
                MontFact = facture.MontFact,
                ExistsInChifa = true,
                Visibility = string.IsNullOrEmpty(facture.NumBord)
                    ? VisibilityStatus.VisibleInFacture
                    : VisibilityStatus.VisibleInBordereau,
                Business = facture.Etat switch
                {
                    "E" => BusinessStatus.Prepared,
                    "V" => BusinessStatus.Validated,
                    "P" => BusinessStatus.Persisted,
                    _ => BusinessStatus.Unknown
                },
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Load invoice snapshot failed for {NumFact}", numFact);
            return null;
        }
    }

    public async Task<List<ChifaInvoiceStatusSnapshot>> LoadAllInvoicesAsync(
        CancellationToken ct = default)
    {
        try
        {
            var factures = await _readContext.ChifaFactures
                .AsNoTracking()
                .OrderByDescending(f => f.DateFact)
                .Take(100)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return factures.Select(f => new ChifaInvoiceStatusSnapshot
            {
                NumFact = f.NumFact,
                Etat = f.Etat,
                NumBord = f.NumBord,
                MontFact = f.MontFact,
                ExistsInChifa = true,
                Timestamp = DateTime.UtcNow
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Load all invoices failed");
            return new List<ChifaInvoiceStatusSnapshot>();
        }
    }
}
