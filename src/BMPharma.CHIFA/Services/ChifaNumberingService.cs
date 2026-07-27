using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace BMPharma.CHIFA.Interfaces;

/// <summary>
/// Atomic counter service for CHIFA invoice and bordereau numbers.
/// Uses PostgreSQL UPDATE...RETURNING for atomicity.
/// </summary>
public interface IChifaNumberingService
{
    Task<string> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default);
    Task<string> PeekNextInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task<string> PeekNextBordereauNumberAsync(CancellationToken cancellationToken = default);
}

public class ChifaNumberingService : IChifaNumberingService
{
    private readonly Microsoft.EntityFrameworkCore.DbContext _context;

    public ChifaNumberingService(Microsoft.EntityFrameworkCore.DbContext context)
    {
        _context = context;
    }

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE parametre SET next_num_fact = next_num_fact + 1 RETURNING next_num_fact";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var num = Convert.ToInt32(result);
        return num.ToString("D8");
    }

    public async Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE parametre SET next_num_bord = next_num_bord + 1 RETURNING next_num_bord";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var num = Convert.ToInt32(result);
        return num.ToString("D6");
    }

    public async Task<string> PeekNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT next_num_fact FROM parametre";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var num = Convert.ToInt32(result);
        return num.ToString("D8");
    }

    public async Task<string> PeekNextBordereauNumberAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT next_num_bord FROM parametre";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var num = Convert.ToInt32(result);
        return num.ToString("D6");
    }
}

/// <summary>
/// Fake numbering service for ReadOnly and test modes.
/// Returns predictable sequential numbers without touching the database.
/// </summary>
public class ChifaNumberingServiceFake : IChifaNumberingService
{
    private int _nextFact = 1;
    private int _nextBord = 1;

    public Task<string> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((_nextFact++).ToString("D8"));

    public Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((_nextBord++).ToString("D6"));

    public Task<string> PeekNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_nextFact.ToString("D8"));

    public Task<string> PeekNextBordereauNumberAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_nextBord.ToString("D6"));
}
