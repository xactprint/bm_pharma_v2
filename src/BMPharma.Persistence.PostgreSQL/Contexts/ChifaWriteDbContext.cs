using Microsoft.EntityFrameworkCore;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;
using BMPharma.Persistence.PostgreSQL.Configurations;

namespace BMPharma.Persistence.PostgreSQL.Contexts;

/// <summary>
/// Write DbContext for CHIFA PostgreSQL database.
/// Used for CHIFA Bordereau creation, signature, and closure operations.
/// Only active in Test or Production mode — never in ReadOnly mode.
/// Schema: public | Source: BM-PHASE-004.9 real database discovery
/// </summary>
public class ChifaWriteDbContext : DbContext
{
    public DbSet<ChifaFacture> Factures => Set<ChifaFacture>();
    public DbSet<ChifaDetailFact> DetailFacts => Set<ChifaDetailFact>();
    public DbSet<ChifaBordereau> Bordereaus => Set<ChifaBordereau>();
    public DbSet<ChifaParametre> Parametres => Set<ChifaParametre>();
    public DbSet<ChifaMedicament> Medicaments => Set<ChifaMedicament>();
    public DbSet<ChifaSignature> Signatures => Set<ChifaSignature>();

    public ChifaWriteDbContext(DbContextOptions<ChifaWriteDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChifaFactureConfiguration).Assembly);
    }
}
