using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

[Table("parametre")]
public class ChifaParametre
{
    [Column("code_ps")]
    public string? CodePs { get; set; }

    [Column("code_centre")]
    public string? CodeCentre { get; set; }

    [Column("nom_pharmacie")]
    public string? NomPharmacie { get; set; }

    [Column("next_num_fact")]
    public int? NextNumFact { get; set; }

    [Column("next_num_bord")]
    public short? NextNumBord { get; set; }

    [Column("adresse")]
    public string? Adresse { get; set; }

    [Column("tel")]
    public string? Tel { get; set; }

    [Column("fax")]
    public string? Fax { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("wilaya")]
    public string? Wilaya { get; set; }

    [Column("commune")]
    public string? Commune { get; set; }

    [Column("code_postal")]
    public string? CodePostal { get; set; }

    [Column("date_creation")]
    public DateTime? DateCreation { get; set; }

    [Column("date_modification")]
    public DateTime? DateModification { get; set; }

    [Column("version")]
    public int? Version { get; set; }
}
