using Microsoft.EntityFrameworkCore;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Contexts;

/// <summary>
/// Read-only DbContext for CHIFA PostgreSQL database.
/// Maps to existing CHIFA tables. No migrations — read-only access.
/// All queries executed through this context are SELECT-only.
/// </summary>
public class ChifaPostgreSqlContext : DbContext
{
    public DbSet<ChifaFacture> Factures => Set<ChifaFacture>();
    public DbSet<ChifaDetailFact> DetailFacts => Set<ChifaDetailFact>();
    public DbSet<ChifaBordereau> Bordereaus => Set<ChifaBordereau>();
    public DbSet<ChifaParametre> Parametres => Set<ChifaParametre>();

    public ChifaPostgreSqlContext(DbContextOptions<ChifaPostgreSqlContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChifaFacture>(entity =>
        {
            entity.HasKey(e => e.NumFact);
            entity.Property(e => e.NumFact).HasMaxLength(8);
            entity.Property(e => e.NumBord).HasMaxLength(6);
            entity.Property(e => e.NumAssure).HasMaxLength(12);
            entity.Property(e => e.CodeCentre).HasMaxLength(5);
            entity.Property(e => e.NatRemb).HasMaxLength(1);
            entity.Property(e => e.Version).HasMaxLength(10);
            entity.Property(e => e.MontOff).HasPrecision(10, 2);
            entity.Property(e => e.MontAs).HasPrecision(10, 2);
            entity.Property(e => e.MontFact).HasPrecision(11, 2);
            entity.Property(e => e.MontMajFae).HasPrecision(4, 2);
            entity.Property(e => e.MontMaj).HasPrecision(11, 2);
            entity.Property(e => e.MontMut).HasPrecision(10, 2);
            entity.Property(e => e.Taux).HasPrecision(5, 2);
        });

        modelBuilder.Entity<ChifaDetailFact>(entity =>
        {
            entity.HasKey(e => new { e.NumFact, e.NumEnr, e.Ppa });
            entity.Property(e => e.NumFact).HasMaxLength(8);
            entity.Property(e => e.NumEnr).HasMaxLength(5);
            entity.Property(e => e.NumEnrPrescrit).HasMaxLength(5);
            entity.Property(e => e.NumLot).HasMaxLength(6);
            entity.Property(e => e.Posologie).HasMaxLength(50);
            entity.Property(e => e.Ppa).HasPrecision(10, 2);
            entity.Property(e => e.Qte).HasPrecision(3, 0);
            entity.Property(e => e.Mont).HasPrecision(10, 2);
            entity.Property(e => e.MontAs).HasPrecision(10, 2);
            entity.Property(e => e.MontPharm).HasPrecision(10, 2);
            entity.Property(e => e.MajLocal).HasPrecision(10, 2);
            entity.Property(e => e.MajSub).HasPrecision(3, 0);
            entity.Property(e => e.DureeTrait).HasPrecision(3, 0);
            entity.Property(e => e.TarifRef).HasPrecision(10, 2);
        });

        modelBuilder.Entity<ChifaBordereau>(entity =>
        {
            entity.HasKey(e => e.IdBord);
            entity.Property(e => e.NumBord).HasMaxLength(6);
            entity.Property(e => e.CodeCentre).HasMaxLength(5);
            entity.Property(e => e.PosteCloture).HasMaxLength(100);
            entity.Property(e => e.MontVir).HasPrecision(10, 2);
        });

        modelBuilder.Entity<ChifaParametre>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<ChifaFacture>().ToTable("facture");
        modelBuilder.Entity<ChifaDetailFact>().ToTable("detail_fact");
        modelBuilder.Entity<ChifaBordereau>().ToTable("bordereau");
        modelBuilder.Entity<ChifaParametre>().ToTable("parametre");
    }
}
