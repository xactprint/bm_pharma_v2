using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

[Table("detail_fact")]
public class ChifaDetailFact
{
    [Column("num_fact")]
    public string NumFact { get; set; } = null!;

    [Column("num_enr")]
    public string NumEnr { get; set; } = null!;

    [Column("ppa")]
    public decimal Ppa { get; set; }

    [Column("qte")]
    public decimal Qte { get; set; }

    [Column("mont")]
    public decimal Mont { get; set; }

    [Column("mont_as")]
    public decimal? MontAs { get; set; }

    [Column("mont_pharm")]
    public decimal? MontPharm { get; set; }

    [Column("num_enr_prescrit")]
    public string NumEnrPrescrit { get; set; } = null!;

    [Column("num_lot")]
    public string? NumLot { get; set; }

    [Column("maj_local")]
    public decimal? MajLocal { get; set; }

    [Column("maj_sub")]
    public decimal? MajSub { get; set; }

    [Column("duree_trait")]
    public decimal? DureeTrait { get; set; }

    [Column("tarif_ref")]
    public decimal? TarifRef { get; set; }

    [Column("posologie")]
    public string? Posologie { get; set; }

    [Column("remboursable")]
    public bool? Remboursable { get; set; }

    [Column("local")]
    public bool? Local { get; set; }

    [Column("inf_tr")]
    public bool? InfTr { get; set; }

    [Column("applic_tr")]
    public bool? ApplicTr { get; set; }

    [Column("medic")]
    public bool? Medic { get; set; }

    [Column("ts")]
    public bool? Ts { get; set; }
}
