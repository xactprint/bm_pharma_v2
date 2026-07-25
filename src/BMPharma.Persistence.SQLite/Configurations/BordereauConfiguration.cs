using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class BordereauConfiguration : IEntityTypeConfiguration<Bordereau>
{
    public void Configure(EntityTypeBuilder<Bordereau> builder)
    {
        builder.ToTable("Bordereaus");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BordereauNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.TotalAmountDA).HasColumnType("decimal(18,2)");
        builder.HasIndex(b => b.BordereauNumber).IsUnique();
        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
