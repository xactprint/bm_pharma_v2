using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Persistence.PostgreSQL.Contexts;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

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
        var correlationId = Guid.NewGuid().ToString("N")[..8];

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
            using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, cancellationToken);

            try
            {
                var numFact = request.NumFact;
                var now = DateTime.UtcNow;
                var dateSoin = request.DateSoin == default ? DateTime.Today : request.DateSoin;

                var montFact = request.Lines.Sum(l => l.Quantite * l.PrixUnit);
                var montAs = Math.Round(montFact * 0.70m, 2);

                _logger.LogInformation("[{CorId}] Creating CHIFA invoice {NumFact} with {LineCount} lines, total {Total}",
                    correlationId, numFact, request.Lines.Count, montFact);

                var facture = new ChifaFacture
                {
                    NumFact = numFact,
                    DateFact = now,
                    Etat = "0",
                    NumBord = null,
                    MontOff = montFact,
                    MontAs = montAs,
                    MontFact = montFact,
                    NumAssure = request.NumAssure,
                    RangAd = "1",
                    CodeCentre = request.CodeCentre.ToString(),
                    Tp = "1",
                    Taux = "3",
                    CodeAffect = "01",
                    Conv = "1",
                    TypeConsult = "01",
                    Prescripteur = "BM PHARMA",
                    DateSoin = dateSoin,
                    Risque = "0",
                    StatutFact = "1",
                    Verifcms = "0",
                    TypeSignature = "0",
                    VerifFact = "0",
                    MontMajFae = 0,
                    MontMaj = 0,
                    TypeMaj = 0,
                    Version = "2.0.0",
                    Signature = null,
                    FactXml = null,
                    Echifa = false,
                    EOrd = false
                };

                _context.Factures.Add(facture);

                foreach (var line in request.Lines)
                {
                    var detail = new ChifaDetailFact
                    {
                        NumFact = numFact,
                        NumEnr = line.NumEnr,
                        Ppa = line.PrixUnit,
                        Qte = line.Quantite,
                        Mont = line.Quantite * line.PrixUnit,
                        MontAs = Math.Round(line.Quantite * line.PrixUnit * 0.70m, 2),
                        MontPharm = Math.Round(line.Quantite * line.PrixUnit * 0.30m, 2),
                        NumEnrPrescrit = line.NumEnr,
                        NumLot = line.NumLot,
                        MajLocal = 0,
                        MajSub = 0,
                        DureeTrait = line.DureeTrait,
                        TarifRef = line.PrixUnit,
                        Posologie = line.Posologie,
                        Remboursable = true,
                        Local = false,
                        InfTr = line.InfTr == 1,
                        ApplicTr = line.ApplicTr == 1,
                        Medic = line.Medic == 1,
                        Ts = false
                    };

                    _context.DetailFacts.Add(detail);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                sw.Stop();
                await _auditService.LogOperationAsync("CREATE_INVOICE", "facture", numFact,
                    $"Lines: {request.Lines.Count}, Total: {montFact}, CorrelationId: {correlationId}",
                    true, sw.ElapsedMilliseconds, cancellationToken: cancellationToken);

                _logger.LogInformation("[{CorId}] CHIFA invoice {NumFact} created successfully in {Elapsed}ms",
                    correlationId, numFact, sw.ElapsedMilliseconds);

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
            _logger.LogError(ex, "[{CorId}] Failed to create CHIFA invoice {NumFact}", correlationId, request.NumFact);
            await _auditService.LogOperationAsync("CREATE_INVOICE", "facture", request.NumFact,
                false, sw.ElapsedMilliseconds, ex.Message, cancellationToken: cancellationToken);
            return new ChifaInvoiceResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> InvoiceExistsInChifaAsync(
        string numFact, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if invoice {NumFact} exists in CHIFA", numFact);
        return await _context.Factures.AnyAsync(f => f.NumFact == numFact, cancellationToken);
    }
}
