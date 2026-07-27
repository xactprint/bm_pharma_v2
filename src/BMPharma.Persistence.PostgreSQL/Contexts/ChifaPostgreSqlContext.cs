using Microsoft.EntityFrameworkCore;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using BMPharma.Persistence.PostgreSQL.Configurations;

namespace BMPharma.Persistence.PostgreSQL.Contexts;

/// <summary>
/// Read-only DbContext for CHIFA PostgreSQL database.
/// Maps to existing CHIFA tables. No migrations — read-only access.
/// All queries executed through this context are SELECT-only.
/// Schema: public | Source: BM-PHASE-004.9 real database discovery
/// </summary>
public class ChifaPostgreSqlContext : DbContext
{
    public DbSet<ChifaFacture> Factures => Set<ChifaFacture>();
    public DbSet<ChifaDetailFact> DetailFacts => Set<ChifaDetailFact>();
    public DbSet<ChifaBordereau> Bordereaus => Set<ChifaBordereau>();
    public DbSet<ChifaParametre> Parametres => Set<ChifaParametre>();
    public DbSet<ChifaMedicament> Medicaments => Set<ChifaMedicament>();
    public DbSet<ChifaSignature> Signatures => Set<ChifaSignature>();

    public ChifaPostgreSqlContext(DbContextOptions<ChifaPostgreSqlContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChifaFactureConfiguration).Assembly);
    }
}
