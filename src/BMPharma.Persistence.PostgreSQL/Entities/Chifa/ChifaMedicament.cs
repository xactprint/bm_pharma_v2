using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

/// <summary>
/// Maps to the real CHIFA_OFFICINE medicament table (29 columns, 7596 rows, 80 MB).
/// Drug catalog — critical for BM Pharma drug lookup, pricing, and reimbursement.
/// Schema: public | Source: BM-PHASE-004.9 real database discovery
/// </summary>
[Table("medicament")]
public class ChifaMedicament
{
    [Column("num_enr")]
    public string NumEnr { get; set; } = null!;

    [Column("nom_com")]
    public string? NomCom { get; set; }

    [Column("nom_dci")]
    public string? NomDci { get; set; }

    [Column("dosage")]
    public string? Dosage { get; set; }

    [Column("unite")]
    public string? Unite { get; set; }

    [Column("conditionnement")]
    public string? Conditionnement { get; set; }

    [Column("convention")]
    public string? Convention { get; set; }

    [Column("remboursable")]
    public string? Remboursable { get; set; }

    [Column("date_remboursement")]
    public string? DateRemboursement { get; set; }

    [Column("date_arret_remboursement")]
    public string? DateArretRemboursement { get; set; }

    [Column("date_decision")]
    public string? DateDecision { get; set; }

    [Column("tarif_ref")]
    public decimal? TarifRef { get; set; }

    [Column("taux")]
    public decimal? Taux { get; set; }

    [Column("code_forme")]
    public string? CodeForme { get; set; }

    [Column("tableau")]
    public string? Tableau { get; set; }

    [Column("hopital")]
    public string? Hopital { get; set; }

    [Column("secteur_sanitaire")]
    public string? SecteurSanitaire { get; set; }

    [Column("officine")]
    public string? Officine { get; set; }

    [Column("pays")]
    public string? Pays { get; set; }

    [Column("laboratoire")]
    public string? Laboratoire { get; set; }

    [Column("cm")]
    public string? Cm { get; set; }

    [Column("code_medic")]
    public string? CodeMedic { get; set; }

    [Column("date_tr")]
    public string? DateTr { get; set; }

    [Column("observation")]
    public string? Observation { get; set; }

    [Column("code_dci")]
    public string? CodeDci { get; set; }

    [Column("code_sp")]
    public string? CodeSp { get; set; }

    [Column("inf_tr")]
    public string? InfTr { get; set; }

    [Column("generic")]
    public string? Generic { get; set; }

    [Column("medic")]
    public string? Medic { get; set; }
}
