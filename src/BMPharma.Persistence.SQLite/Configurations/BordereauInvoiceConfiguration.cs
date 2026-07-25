using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class BordereauInvoiceConfiguration : IEntityTypeConfiguration<BordereauInvoice>
{
    public void Configure(EntityTypeBuilder<BordereauInvoice> builder)
    {
        builder.ToTable("BordereauInvoices");
        builder.HasKey(bi => bi.Id);
        builder.HasOne(bi => bi.Bordereau)
            .WithMany(b => b.BordereauInvoices)
            .HasForeignKey(bi => bi.BordereauId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(bi => bi.Invoice)
            .WithMany()
            .HasForeignKey(bi => bi.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
