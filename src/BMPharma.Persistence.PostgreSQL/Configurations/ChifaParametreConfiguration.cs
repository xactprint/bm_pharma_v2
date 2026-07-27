using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Configurations;

public class ChifaParametreConfiguration : IEntityTypeConfiguration<ChifaParametre>
{
    public void Configure(EntityTypeBuilder<ChifaParametre> builder)
    {
        builder.ToTable("parametre");
        builder.HasNoKey();

        builder.Property(e => e.CodePs).HasMaxLength(10);
        builder.Property(e => e.NomPharmacie).HasMaxLength(50);
        builder.Property(e => e.Nom).HasMaxLength(25);
        builder.Property(e => e.Prenom).HasMaxLength(25);
        builder.Property(e => e.Adresse).HasMaxLength(50);
        builder.Property(e => e.NumTel).HasMaxLength(20);
        builder.Property(e => e.NumFax).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(50);
        builder.Property(e => e.CodeSp).HasMaxLength(2);
        builder.Property(e => e.Nis).HasMaxLength(15);
        builder.Property(e => e.Nico).HasMaxLength(14);
        builder.Property(e => e.Ndps).HasMaxLength(14);
        builder.Property(e => e.CodeCentre).HasMaxLength(5);
        builder.Property(e => e.Convention).HasMaxLength(1);
        builder.Property(e => e.RefConvention).HasMaxLength(25);
        builder.Property(e => e.RefBancaire).HasMaxLength(20);
        builder.Property(e => e.ModeReglement).HasMaxLength(1);
        builder.Property(e => e.Contact).HasMaxLength(40);
        builder.Property(e => e.Version).HasMaxLength(20);
        builder.Property(e => e.DateConvention).HasMaxLength(10);
        builder.Property(e => e.CheminBackup).HasMaxLength(200);
        builder.Property(e => e.HeureBackup).HasMaxLength(5);
        builder.Property(e => e.PosteTelech).HasMaxLength(30);
        builder.Property(e => e.Params).HasMaxLength(255);
        builder.Property(e => e.AccessToken).HasMaxLength(256);
        builder.Property(e => e.RefreshToken).HasMaxLength(256);

        builder.Property(e => e.MontMax).HasPrecision(8, 2);
        builder.Property(e => e.MontMajFae).HasPrecision(2, 0);
        builder.Property(e => e.MontMajSub).HasPrecision(2, 0);
        builder.Property(e => e.TauxMajLocal).HasPrecision(2, 0);
        builder.Property(e => e.TauxMajInfTr).HasPrecision(2, 0);
    }
}
