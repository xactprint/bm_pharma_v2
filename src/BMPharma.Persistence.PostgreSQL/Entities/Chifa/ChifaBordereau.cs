using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

[Table("bordereau")]
public class ChifaBordereau
{
    [Column("id_bord")]
    public long IdBord { get; set; }

    [Column("num_bord")]
    public string NumBord { get; set; } = null!;

    [Column("code_centre")]
    public string CodeCentre { get; set; } = null!;

    [Column("etat")]
    public string? Etat { get; set; }

    [Column("id_user_cloture")]
    public int? IdUserCloture { get; set; }

    [Column("poste_cloture")]
    public string? PosteCloture { get; set; }

    [Column("mont_vir")]
    public decimal? MontVir { get; set; }

    [Column("duplicata")]
    public bool? Duplicata { get; set; }

    [Column("date_cloture")]
    public DateTime? DateCloture { get; set; }

    [Column("date_ouverture")]
    public DateTime? DateOuverture { get; set; }

    [Column("date_depot_ftp")]
    public DateTime? DateDepotFtp { get; set; }
}
