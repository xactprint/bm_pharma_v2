using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Persistence.PostgreSQL.Contexts;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using Npgsql;

namespace BMPharma.CHIFA.Services;

public class ChifaPostgresBordereauService : IChifaBordereauService
{
    private readonly ChifaWriteDbContext _context;
    private readonly ChifaWriteGuard _guard;
    private readonly ChifaBordereauValidator _validator;
    private readonly IChifaAuditService _auditService;
    private readonly ILogger<ChifaPostgresBordereauService> _logger;

    public ChifaPostgresBordereauService(
        ChifaWriteDbContext context,
        ChifaWriteGuard guard,
        ChifaBordereauValidator validator,
        IChifaAuditService auditService,
        ILogger<ChifaPostgresBordereauService> logger)
    {
        _context = context;
        _guard = guard;
        _validator = validator;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ChifaBordereauResult> CreateBordereauAsync(
        ChifaBordereauRequest request, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _guard.EnsureWriteAllowedAsync();
        }
        catch (ChifaWriteBlockedException ex)
        {
            sw.Stop();
            await _auditService.LogOperationAsync("CREATE_BORDEREAU", "bordereau", request.NumBord,
                false, sw.ElapsedMilliseconds, ex.Message, cancellationToken: cancellationToken);
            return new ChifaBordereauResult { Success = false, ErrorMessage = ex.Message };
        }

        var validationResult = _validator.Validate(request.NumBord, "11600", request.InvoiceNumbers);
        if (!validationResult.IsValid)
        {
            var errorMsg = string.Join("; ", validationResult.Errors.Select(e => e.Message));
            sw.Stop();
            await _auditService.LogOperationAsync("VALIDATE_BORDEREAU", "bordereau", request.NumBord,
                errorMsg, false, sw.ElapsedMilliseconds, cancellationToken: cancellationToken);
            return new ChifaBordereauResult { Success = false, ErrorMessage = errorMsg };
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, cancellationToken);

            try
            {
                _logger.LogInformation("Creating CHIFA bordereau {NumBord} with {InvoiceCount} invoices",
                    request.NumBord, request.InvoiceNumbers.Count);

                var bordereau = new ChifaBordereau
                {
                    NumBord = request.NumBord,
                    CodeCentre = "11600",
                    Etat = "0",
                    DateOuverture = DateTime.UtcNow,
                    Duplicata = false
                };

                _context.Bordereaus.Add(bordereau);

                foreach (var numFact in request.InvoiceNumbers)
                {
                    var facture = await _context.Factures
                        .FirstOrDefaultAsync(f => f.NumFact == numFact, cancellationToken);

                    if (facture != null)
                    {
                        facture.NumBord = request.NumBord;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                sw.Stop();
                await _auditService.LogOperationAsync("CREATE_BORDEREAU", "bordereau", request.NumBord,
                    $"Invoices: {request.InvoiceNumbers.Count}",
                    true, sw.ElapsedMilliseconds, cancellationToken: cancellationToken);

                return new ChifaBordereauResult
                {
                    Success = true,
                    NumBord = request.NumBord
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
            _logger.LogError(ex, "Failed to create CHIFA bordereau {NumBord}", request.NumBord);
            await _auditService.LogOperationAsync("CREATE_BORDEREAU", "bordereau", request.NumBord,
                false, sw.ElapsedMilliseconds, ex.Message, cancellationToken: cancellationToken);
            return new ChifaBordereauResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting next bordereau number via atomic UPDATE");

        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE parametre SET next_num_bord = next_num_bord + 1 RETURNING next_num_bord";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var num = Convert.ToInt32(result);
        return num.ToString("D6");
    }

    public Task<ChifaBordereauResult> SignBordereauAsync(
        string numBord, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Signing is delegated to CHIFA-OFFICINE. BM Pharma does not sign.");
        return Task.FromResult(new ChifaBordereauResult
        {
            Success = false,
            ErrorMessage = "Signing must be performed through CHIFA-OFFICINE using the professional token. " +
                          "BM Pharma cannot perform cryptographic signing operations."
        });
    }

    public Task<ChifaBordereauResult> CloseBordereauAsync(
        string numBord, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Closure is delegated to CHIFA-OFFICINE. BM Pharma does not close.");
        return Task.FromResult(new ChifaBordereauResult
        {
            Success = false,
            ErrorMessage = "Bordereau closure must be performed through CHIFA-OFFICINE. " +
                          "BM Pharma cannot close bordereaux that require CNAS transmission."
        });
    }
}
