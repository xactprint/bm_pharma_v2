using BMPharma.CHIFA.Interfaces;
using BMPharma.CHIFA.Services;
using BMPharma.Domain.Enums;
using BMPharma.Persistence.PostgreSQL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== BM-PHASE-007-E — InMemory Simulation Test ===\n");

        var options = new DbContextOptionsBuilder<ChifaWriteDbContext>()
            .UseInMemoryDatabase(databaseName: "ChifaSimulation_" + Guid.NewGuid())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());

        await using var context = new ChifaWriteDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var guard = new ChifaWriteGuard(() => Task.FromResult(ChifaIntegrationMode.Test));
        var validator = new ChifaInvoiceValidator(loggerFactory.CreateLogger<ChifaInvoiceValidator>());
        var audit = new ChifaAuditService(loggerFactory.CreateLogger<ChifaAuditService>());

        var service = new ChifaPostgresInvoiceService(
            context, guard, validator, audit,
            loggerFactory.CreateLogger<ChifaPostgresInvoiceService>());

        var request = new ChifaInvoiceRequest
        {
            NumFact = "TST002",
            NumAssure = "TST99999",
            CodeCentre = 11600,
            DateSoin = DateTime.Now,
            Lines = new List<ChifaInvoiceLineRequest>
            {
                new ChifaInvoiceLineRequest
                {
                    NumEnr = "00010",
                    MedicCode = 10,
                    PrixUnit = 60.00m,
                    Quantite = 2,
                    NumLot = "LOT002",
                    Posologie = "1/day",
                    InfTr = 1,
                    ApplicTr = 1,
                    Medic = 1,
                    Ts = 4,
                    DureeTrait = 30
                }
            }
        };

        Console.WriteLine("Calling CreateInvoiceAsync...");
        ChifaInvoiceResult result;

        try
        {
            result = await service.CreateInvoiceAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION: {ex.Message}");
            Console.WriteLine($"\n=== SIMULATION FAILED ===");
            return;
        }

        Console.WriteLine($"Success: {result.Success}");
        if (!result.Success)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
            Console.WriteLine($"\n=== SIMULATION FAILED ===");
            return;
        }

        Console.WriteLine($"NumFact: {result.ChifaNumFact}");

        var facture = await context.Factures.FindAsync("TST002");
        if (facture == null)
        {
            Console.WriteLine("FAIL: facture not found in InMemory DB");
            return;
        }

        Console.WriteLine($"\nFacture saved:");
        Console.WriteLine($"  NumFact: {facture.NumFact}");
        Console.WriteLine($"  NumAssure: {facture.NumAssure}");
        Console.WriteLine($"  CodeCentre: {facture.CodeCentre}");
        Console.WriteLine($"  DateSoin: {facture.DateSoin}");
        Console.WriteLine($"  MontFact: {facture.MontFact}");
        Console.WriteLine($"  MontAs: {facture.MontAs}");

        var details = await context.DetailFacts
            .Where(d => d.NumFact == "TST002")
            .ToListAsync();

        Console.WriteLine($"\nDetail lines: {details.Count}");
        foreach (var d in details)
        {
            Console.WriteLine($"  {d.NumEnr} | PPA: {d.Ppa} | Qte: {d.Qte} | Mont: {d.Mont}");
        }

        Console.WriteLine($"\n=== SIMULATION PASSED — All checks OK ===");
    }
}
