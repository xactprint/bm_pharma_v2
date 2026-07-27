using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Configurations;

public class ChifaDetailFactConfiguration : IEntityTypeConfiguration<ChifaDetailFact>
{
    public void Configure(EntityTypeBuilder<ChifaDetailFact> builder)
    {
        builder.ToTable("detail_fact");
        builder.HasKey(e => new { e.NumFact, e.NumEnr, e.Ppa });

        builder.Property(e => e.NumFact).HasMaxLength(8);
        builder.Property(e => e.NumEnr).HasMaxLength(5);
        builder.Property(e => e.NumEnrPrescrit).HasMaxLength(5);
        builder.Property(e => e.NumLot).HasMaxLength(6);
        builder.Property(e => e.Posologie).HasMaxLength(50);

        builder.Property(e => e.Ppa).HasPrecision(10, 2);
        builder.Property(e => e.Qte).HasPrecision(3, 0);
        builder.Property(e => e.Mont).HasPrecision(10, 2);
        builder.Property(e => e.MontAs).HasPrecision(10, 2);
        builder.Property(e => e.MontPharm).HasPrecision(10, 2);
        builder.Property(e => e.MajLocal).HasPrecision(10, 2);
        builder.Property(e => e.MajSub).HasPrecision(3, 0);
        builder.Property(e => e.DureeTrait).HasPrecision(3, 0);
        builder.Property(e => e.TarifRef).HasPrecision(10, 2);
    }
}
