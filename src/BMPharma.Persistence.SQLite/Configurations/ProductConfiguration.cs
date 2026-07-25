using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(p => p.NameFr).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PriceDA).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PurchasePriceDA).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ReimbursementRate).HasColumnType("decimal(5,2)");
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.NameFr);
        builder.HasIndex(p => p.NameAr);
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
