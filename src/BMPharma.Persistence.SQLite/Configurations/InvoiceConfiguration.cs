using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.SubTotalDA).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxDA).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalDA).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountDA).HasColumnType("decimal(18,2)");
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.InvoiceDate);
        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
