using Microsoft.EntityFrameworkCore;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

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

        modelBuilder.Entity<ChifaFacture>(entity =>
        {
            entity.HasKey(e => e.NumFact);
            entity.Property(e => e.NumFact).HasMaxLength(8);
            entity.Property(e => e.NumBord).HasMaxLength(6);
            entity.Property(e => e.NumAssure).HasMaxLength(12);
            entity.Property(e => e.RangAd).HasMaxLength(2);
            entity.Property(e => e.CodeCentre).HasMaxLength(5);
            entity.Property(e => e.Tp).HasMaxLength(1);
            entity.Property(e => e.Taux).HasMaxLength(1);
            entity.Property(e => e.CodeAffect).HasMaxLength(2);
            entity.Property(e => e.Conv).HasMaxLength(1);
            entity.Property(e => e.TypeConsult).HasMaxLength(2);
            entity.Property(e => e.Prescripteur).HasMaxLength(50);
            entity.Property(e => e.Risque).HasMaxLength(1);
            entity.Property(e => e.StatutFact).HasMaxLength(1);
            entity.Property(e => e.Verifcms).HasMaxLength(1);
            entity.Property(e => e.TypeSignature).HasMaxLength(1);
            entity.Property(e => e.VerifFact).HasMaxLength(1);
            entity.Property(e => e.MontOff).HasPrecision(10, 2);
            entity.Property(e => e.MontAs).HasPrecision(10, 2);
            entity.Property(e => e.MontFact).HasPrecision(11, 2);
            entity.Property(e => e.MontMajFae).HasPrecision(4, 2);
            entity.Property(e => e.MontMaj).HasPrecision(11, 2);
            entity.Property(e => e.CodeCentreAs).HasMaxLength(5);
            entity.Property(e => e.CodeSp).HasMaxLength(2);
            entity.Property(e => e.TypeOrd).HasMaxLength(1);
            entity.Property(e => e.MotifMed).HasMaxLength(16);
            entity.Property(e => e.Signature).HasMaxLength(4000);
            entity.Property(e => e.CodeCovid).HasMaxLength(20);
            entity.Property(e => e.FactXml).HasMaxLength(4000);
            entity.Property(e => e.NatRemb).HasMaxLength(1);
            entity.Property(e => e.MontMut).HasPrecision(10, 2);
            entity.Property(e => e.CodeMut).HasMaxLength(2);
            entity.Property(e => e.AdresseIp).HasMaxLength(15);
            entity.Property(e => e.NomPc).HasMaxLength(30);
            entity.Property(e => e.Obs).HasMaxLength(255);
            entity.Property(e => e.RefCm).HasMaxLength(18);
            entity.Property(e => e.Version).HasMaxLength(10);
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
            entity.Property(e => e.CodePs).HasMaxLength(10);
            entity.Property(e => e.NomPharmacie).HasMaxLength(50);
            entity.Property(e => e.Nom).HasMaxLength(25);
            entity.Property(e => e.Prenom).HasMaxLength(25);
            entity.Property(e => e.Adresse).HasMaxLength(50);
            entity.Property(e => e.NumTel).HasMaxLength(20);
            entity.Property(e => e.NumFax).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.CodeSp).HasMaxLength(2);
            entity.Property(e => e.Nis).HasMaxLength(15);
            entity.Property(e => e.Nico).HasMaxLength(14);
            entity.Property(e => e.Ndps).HasMaxLength(14);
            entity.Property(e => e.CodeCentre).HasMaxLength(5);
            entity.Property(e => e.Convention).HasMaxLength(1);
            entity.Property(e => e.RefConvention).HasMaxLength(25);
            entity.Property(e => e.RefBancaire).HasMaxLength(20);
            entity.Property(e => e.ModeReglement).HasMaxLength(1);
            entity.Property(e => e.MontMax).HasPrecision(8, 2);
            entity.Property(e => e.Contact).HasMaxLength(40);
            entity.Property(e => e.MontMajFae).HasPrecision(2, 0);
            entity.Property(e => e.MontMajSub).HasPrecision(2, 0);
            entity.Property(e => e.TauxMajLocal).HasPrecision(2, 0);
            entity.Property(e => e.TauxMajInfTr).HasPrecision(2, 0);
            entity.Property(e => e.Version).HasMaxLength(20);
            entity.Property(e => e.DateConvention).HasMaxLength(10);
            entity.Property(e => e.CheminBackup).HasMaxLength(200);
            entity.Property(e => e.HeureBackup).HasMaxLength(5);
            entity.Property(e => e.PosteTelech).HasMaxLength(30);
            entity.Property(e => e.Params).HasMaxLength(255);
            entity.Property(e => e.AccessToken).HasMaxLength(256);
            entity.Property(e => e.RefreshToken).HasMaxLength(256);
        });

        modelBuilder.Entity<ChifaMedicament>(entity =>
        {
            entity.HasKey(e => e.NumEnr);
            entity.Property(e => e.NumEnr).HasMaxLength(5);
            entity.Property(e => e.NomCom).HasMaxLength(50);
            entity.Property(e => e.NomDci).HasMaxLength(60);
            entity.Property(e => e.Dosage).HasMaxLength(30);
            entity.Property(e => e.Unite).HasMaxLength(20);
            entity.Property(e => e.Conditionnement).HasMaxLength(20);
            entity.Property(e => e.Convention).HasMaxLength(1);
            entity.Property(e => e.Remboursable).HasMaxLength(1);
            entity.Property(e => e.DateRemboursement).HasMaxLength(10);
            entity.Property(e => e.DateArretRemboursement).HasMaxLength(10);
            entity.Property(e => e.DateDecision).HasMaxLength(10);
            entity.Property(e => e.TarifRef).HasPrecision(11, 2);
            entity.Property(e => e.Taux).HasPrecision(3, 0);
            entity.Property(e => e.CodeForme).HasMaxLength(3);
            entity.Property(e => e.Tableau).HasMaxLength(1);
            entity.Property(e => e.Hopital).HasMaxLength(1);
            entity.Property(e => e.SecteurSanitaire).HasMaxLength(1);
            entity.Property(e => e.Officine).HasMaxLength(1);
            entity.Property(e => e.Pays).HasMaxLength(20);
            entity.Property(e => e.Laboratoire).HasMaxLength(25);
            entity.Property(e => e.Cm).HasMaxLength(1);
            entity.Property(e => e.CodeMedic).HasMaxLength(11);
            entity.Property(e => e.DateTr).HasMaxLength(10);
            entity.Property(e => e.Observation).HasMaxLength(2000);
            entity.Property(e => e.CodeDci).HasMaxLength(6);
            entity.Property(e => e.CodeSp).HasMaxLength(2);
            entity.Property(e => e.InfTr).HasMaxLength(1);
            entity.Property(e => e.Generic).HasMaxLength(1);
            entity.Property(e => e.Medic).HasMaxLength(1);
        });

        modelBuilder.Entity<ChifaSignature>(entity =>
        {
            entity.HasKey(e => e.NumFact);
            entity.Property(e => e.NumFact).HasMaxLength(8);
            entity.Property(e => e.Sign).HasMaxLength(4000);
        });

        modelBuilder.Entity<ChifaFacture>().ToTable("facture");
        modelBuilder.Entity<ChifaDetailFact>().ToTable("detail_fact");
        modelBuilder.Entity<ChifaBordereau>().ToTable("bordereau");
        modelBuilder.Entity<ChifaParametre>().ToTable("parametre");
        modelBuilder.Entity<ChifaMedicament>().ToTable("medicament");
        modelBuilder.Entity<ChifaSignature>().ToTable("signature");
    }
}
