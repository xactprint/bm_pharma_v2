using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Configurations;

public class ChifaFactureConfiguration : IEntityTypeConfiguration<ChifaFacture>
{
    public void Configure(EntityTypeBuilder<ChifaFacture> builder)
    {
        builder.ToTable("facture");
        builder.HasKey(e => e.NumFact);

        builder.Property(e => e.NumFact).HasMaxLength(8);
        builder.Property(e => e.NumBord).HasMaxLength(6);
        builder.Property(e => e.NumAssure).HasMaxLength(12);
        builder.Property(e => e.RangAd).HasMaxLength(2);
        builder.Property(e => e.CodeCentre).HasMaxLength(5);
        builder.Property(e => e.Tp).HasMaxLength(1);
        builder.Property(e => e.Taux).HasMaxLength(1);
        builder.Property(e => e.CodeAffect).HasMaxLength(2);
        builder.Property(e => e.Conv).HasMaxLength(1);
        builder.Property(e => e.TypeConsult).HasMaxLength(2);
        builder.Property(e => e.Prescripteur).HasMaxLength(50);
        builder.Property(e => e.Risque).HasMaxLength(1);
        builder.Property(e => e.StatutFact).HasMaxLength(1);
        builder.Property(e => e.Verifcms).HasMaxLength(1);
        builder.Property(e => e.TypeSignature).HasMaxLength(1);
        builder.Property(e => e.VerifFact).HasMaxLength(1);
        builder.Property(e => e.CodeCentreAs).HasMaxLength(5);
        builder.Property(e => e.CodeSp).HasMaxLength(2);
        builder.Property(e => e.TypeOrd).HasMaxLength(1);
        builder.Property(e => e.MotifMed).HasMaxLength(16);
        builder.Property(e => e.Signature).HasColumnType("xml");
        builder.Property(e => e.CodeCovid).HasMaxLength(20);
        builder.Property(e => e.FactXml).HasColumnType("xml");

        builder.Property(e => e.DateFact).HasColumnType("timestamp without time zone");
        builder.Property(e => e.DateSoin).HasColumnType("date");

        builder.Property(e => e.NatRemb).HasMaxLength(1);
        builder.Property(e => e.CodeMut).HasMaxLength(2);
        builder.Property(e => e.AdresseIp).HasMaxLength(15);
        builder.Property(e => e.NomPc).HasMaxLength(30);
        builder.Property(e => e.Obs).HasMaxLength(255);
        builder.Property(e => e.RefCm).HasMaxLength(18);
        builder.Property(e => e.Version).HasMaxLength(10);

        builder.Property(e => e.MontOff).HasPrecision(10, 2);
        builder.Property(e => e.MontAs).HasPrecision(10, 2);
        builder.Property(e => e.MontFact).HasPrecision(11, 2);
        builder.Property(e => e.MontMajFae).HasPrecision(4, 2);
        builder.Property(e => e.MontMaj).HasPrecision(11, 2);
        builder.Property(e => e.MontMut).HasPrecision(10, 2);
    }
}
