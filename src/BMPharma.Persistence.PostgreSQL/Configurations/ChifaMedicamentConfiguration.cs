using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BMPharma.Persistence.PostgreSQL.Entities.Chifa;

namespace BMPharma.Persistence.PostgreSQL.Configurations;

public class ChifaMedicamentConfiguration : IEntityTypeConfiguration<ChifaMedicament>
{
    public void Configure(EntityTypeBuilder<ChifaMedicament> builder)
    {
        builder.ToTable("medicament");
        builder.HasKey(e => e.NumEnr);

        builder.Property(e => e.NumEnr).HasMaxLength(5);
        builder.Property(e => e.NomCom).HasMaxLength(50);
        builder.Property(e => e.NomDci).HasMaxLength(60);
        builder.Property(e => e.Dosage).HasMaxLength(30);
        builder.Property(e => e.Unite).HasMaxLength(20);
        builder.Property(e => e.Conditionnement).HasMaxLength(20);
        builder.Property(e => e.Convention).HasMaxLength(1);
        builder.Property(e => e.Remboursable).HasMaxLength(1);
        builder.Property(e => e.DateRemboursement).HasMaxLength(10);
        builder.Property(e => e.DateArretRemboursement).HasMaxLength(10);
        builder.Property(e => e.DateDecision).HasMaxLength(10);
        builder.Property(e => e.CodeForme).HasMaxLength(3);
        builder.Property(e => e.Tableau).HasMaxLength(1);
        builder.Property(e => e.Hopital).HasMaxLength(1);
        builder.Property(e => e.SecteurSanitaire).HasMaxLength(1);
        builder.Property(e => e.Officine).HasMaxLength(1);
        builder.Property(e => e.Pays).HasMaxLength(20);
        builder.Property(e => e.Laboratoire).HasMaxLength(25);
        builder.Property(e => e.Cm).HasMaxLength(1);
        builder.Property(e => e.CodeMedic).HasMaxLength(11);
        builder.Property(e => e.DateTr).HasMaxLength(10);
        builder.Property(e => e.Observation).HasMaxLength(2000);
        builder.Property(e => e.CodeDci).HasMaxLength(6);
        builder.Property(e => e.CodeSp).HasMaxLength(2);
        builder.Property(e => e.InfTr).HasMaxLength(1);
        builder.Property(e => e.Generic).HasMaxLength(1);
        builder.Property(e => e.Medic).HasMaxLength(1);

        builder.Property(e => e.TarifRef).HasPrecision(11, 2);
        builder.Property(e => e.Taux).HasPrecision(3, 0);
    }
}
