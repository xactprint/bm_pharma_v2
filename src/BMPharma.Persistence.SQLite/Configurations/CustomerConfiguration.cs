using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NationalId).HasMaxLength(20);
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);
        builder.HasIndex(c => c.NationalId);
        builder.HasIndex(c => c.FullName);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
