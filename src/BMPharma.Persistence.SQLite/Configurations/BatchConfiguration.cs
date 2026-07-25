using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("Batches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.LotNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.UnitPriceDA).HasColumnType("decimal(18,2)");
        builder.HasIndex(b => b.LotNumber);
        builder.HasIndex(b => b.ExpiryDate);
        builder.HasOne(b => b.Product)
            .WithMany(p => p.Batches)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
