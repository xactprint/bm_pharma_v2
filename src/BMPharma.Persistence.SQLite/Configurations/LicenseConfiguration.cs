using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Domain.Entities;

namespace BMPharma.Persistence.SQLite.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LicenseKey).IsRequired().HasMaxLength(500);
        builder.Property(l => l.MachineFingerprint).IsRequired().HasMaxLength(100);
    }
}
