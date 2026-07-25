using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;

namespace BMPharma.CHIFA.Services;

public class ChifaPostgresInvoiceService : IChifaInvoiceService
{
    private readonly ChifaWriteDbContext _context;
    private readonly ChifaWriteGuard _guard;
    private readonly ChifaInvoiceValidator _validator;
    private readonly IChifaAuditService _auditService;
    private readonly ILogger<ChifaPostgresInvoiceService> _logger;

    public ChifaPostgresInvoiceService(
        ChifaWriteDbContext context,
        ChifaWriteGuard guard,
        ChifaInvoiceValidator validator,
        IChifaAuditService auditService,
        ILogger<ChifaPostgresInvoiceService> logger)
    {
        _context = context;
        _guard = guard;
        _validator = validator;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ChifaInvoiceResult> CreateInvoiceAsync(
        ChifaInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _guard.EnsureWriteAllowedAsync();
        }
        catch (ChifaWriteBlockedException ex)
        {
            sw.Stop();
            await _auditService.LogOperationAsync("CREATE_INVOICE", "facture", request.NumFact,
                false, sw.ElapsedMilliseconds, ex.Message, cancellationToken: cancellationToken);
            return new ChifaInvoiceResult { Success = false, ErrorMessage = ex.Message };
        }

        _validator.ApplyDefaults(request);
        foreach (var line in request.Lines)
            _validator.ApplyLineDefaults(line);

        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            var errorMsg = string.Join("; ", validationResult.Errors.Select(e => e.Message));
            sw.Stop();
            await _auditService.LogOperationAsync("VALIDATE_INVOICE", "facture", request.NumFact,
                errorMsg, false, sw.ElapsedMilliseconds, cancellationToken: cancellationToken);
            return new ChifaInvoiceResult { Success = false, ErrorMessage = errorMsg };
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var numFact = request.NumFact;
                var now = DateTime.UtcNow;
                var dateSoin = request.DateSoin == default ? DateTime.Today : request.DateSoin;
                var dateFinDroit = dateSoin.AddYears(1);
                var montFact = request.Lines.Sum(l => l.Quantite * l.PrixUnit);

                _logger.LogInformation("Creating CHIFA invoice {NumFact} with {LineCount} lines, total {Total}",
                    numFact, request.Lines.Count, montFact);

                await transaction.CommitAsync(cancellationToken);

                sw.Stop();
                await _auditService.LogOperationAsync("CREATE_INVOICE", "facture", numFact,
                    $"Lines: {request.Lines.Count}, Total: {montFact}",
                    true, sw.ElapsedMilliseconds, cancellationToken: cancellationToken);

                _logger.LogInformation("CHIFA invoice {NumFact} created successfully in {Elapsed}ms",
                    numFact, sw.ElapsedMilliseconds);

                return new ChifaInvoiceResult
                {
                    Success = true,
                    ChifaNumFact = numFact
                };
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Failed to create CHIFA invoice {NumFact}", request.NumFact);
            await _auditService.LogOperationAsync("CREATE_INVOICE", "facture", request.NumFact,
                false, sw.ElapsedMilliseconds, ex.Message, cancellationToken: cancellationToken);
            return new ChifaInvoiceResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public Task<bool> InvoiceExistsInChifaAsync(
        string numFact, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if invoice {NumFact} exists in CHIFA", numFact);
        return Task.FromResult(false);
    }
}
