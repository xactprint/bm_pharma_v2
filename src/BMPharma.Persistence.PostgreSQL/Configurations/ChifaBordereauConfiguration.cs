using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Configurations;

public class ChifaBordereauConfiguration : IEntityTypeConfiguration<ChifaBordereau>
{
    public void Configure(EntityTypeBuilder<ChifaBordereau> builder)
    {
        builder.ToTable("bordereau");
        builder.HasKey(e => e.IdBord);

        builder.Property(e => e.NumBord).HasMaxLength(6);
        builder.Property(e => e.CodeCentre).HasMaxLength(5);
        builder.Property(e => e.PosteCloture).HasMaxLength(100);
        builder.Property(e => e.MontVir).HasPrecision(10, 2);

        builder.HasIndex(e => e.NumBord).IsUnique().HasDatabaseName("UN_BORDEREAU");
    }
}
