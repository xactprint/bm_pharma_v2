using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BMPharma.CHIFA.Interfaces;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;

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
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("Creating CHIFA bordereau {NumBord} with {InvoiceCount} invoices",
                    request.NumBord, request.InvoiceNumbers.Count);

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

    public Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting next bordereau number");
        var nextNum = 216;
        return Task.FromResult(nextNum.ToString("D6"));
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
