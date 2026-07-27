using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Configurations;

public class ChifaSignatureConfiguration : IEntityTypeConfiguration<ChifaSignature>
{
    public void Configure(EntityTypeBuilder<ChifaSignature> builder)
    {
        builder.ToTable("signature");
        builder.HasKey(e => e.NumFact);

        builder.Property(e => e.NumFact).HasMaxLength(8);
        builder.Property(e => e.Sign).HasMaxLength(4000);
    }
}
