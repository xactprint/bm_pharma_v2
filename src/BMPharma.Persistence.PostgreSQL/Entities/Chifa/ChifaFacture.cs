using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

[Table("facture")]
public class ChifaFacture
{
    [Column("num_fact")]
    public string NumFact { get; set; } = null!;

    [Column("date_fact")]
    public DateTime? DateFact { get; set; }

    [Column("etat")]
    public string? Etat { get; set; }

    [Column("num_bord")]
    public string? NumBord { get; set; }

    [Column("mont_off")]
    public decimal? MontOff { get; set; }

    [Column("mont_as")]
    public decimal? MontAs { get; set; }

    [Column("mont_fact")]
    public decimal? MontFact { get; set; }

    [Column("num_assure")]
    public string? NumAssure { get; set; }

    [Column("code_centre")]
    public string? CodeCentre { get; set; }

    [Column("date_soin")]
    public DateTime? DateSoin { get; set; }

    [Column("date_fin_droit")]
    public DateTime? DateFinDroit { get; set; }

    [Column("date_fin_droit_benef")]
    public DateTime? DateFinDroitBenef { get; set; }

    [Column("date_envoi_sms")]
    public DateTime? DateEnvoiSms { get; set; }

    [Column("date_synchro")]
    public DateTime? DateSynchro { get; set; }

    [Column("type_maj")]
    public int TypeMaj { get; set; }

    [Column("mont_maj_fae")]
    public decimal MontMajFae { get; set; }

    [Column("mont_maj")]
    public decimal MontMaj { get; set; }

    [Column("nat_remb")]
    public string? NatRemb { get; set; }

    [Column("mont_mut")]
    public decimal? MontMut { get; set; }

    [Column("date_fin_mut")]
    public DateTime? DateFinMut { get; set; }

    [Column("version")]
    public string? Version { get; set; }

    [Column("num_serie_ps")]
    public long? NumSeriePs { get; set; }

    [Column("version_carte")]
    public int? VersionCarte { get; set; }

    [Column("echifa")]
    public bool? Echifa { get; set; }

    [Column("id_fact_echifa")]
    public long? IdFactEchifa { get; set; }

    [Column("e_ord")]
    public bool? EOrd { get; set; }

    [Column("id_e_ord")]
    public long? IdEOrd { get; set; }

    [Column("taux")]
    public decimal? Taux { get; set; }

    [Column("nom_assure")]
    public string? NomAssure { get; set; }

    [Column("prenom_assure")]
    public string? PrenomAssure { get; set; }

    [Column("nom_benef")]
    public string? NomBenef { get; set; }

    [Column("prenom_benef")]
    public string? PrenomBenef { get; set; }

    [Column("lieu_naissance")]
    public string? LieuNaissance { get; set; }

    [Column("date_naissance")]
    public DateTime? DateNaissance { get; set; }

    [Column("wilaya")]
    public string? Wilaya { get; set; }

    [Column("commune")]
    public string? Commune { get; set; }

    [Column("adresse")]
    public string? Adresse { get; set; }

    [Column("code_postal")]
    public string? CodePostal { get; set; }

    [Column("tel")]
    public string? Tel { get; set; }

    [Column("num_dossier")]
    public string? NumDossier { get; set; }

    [Column("motif_rejet")]
    public string? MotifRejet { get; set; }

    [Column("date_rejet")]
    public DateTime? DateRejet { get; set; }

    [Column("date_paiement")]
    public DateTime? DatePaiement { get; set; }

    [Column("mont_paiement")]
    public decimal? MontPaiement { get; set; }

    [Column("num_cheque")]
    public string? NumCheque { get; set; }

    [Column("date_controle")]
    public DateTime? DateControle { get; set; }

    [Column("id_utilisateur")]
    public int? IdUtilisateur { get; set; }

    [Column("date_creation")]
    public DateTime? DateCreation { get; set; }

    [Column("date_modification")]
    public DateTime? DateModification { get; set; }

    [Column("centre_gestion")]
    public string? CentreGestion { get; set; }

    [Column("code_acte")]
    public string? CodeActe { get; set; }

    [Column("beneficiaire")]
    public string? Beneficiaire { get; set; }

    [Column("matricule")]
    public string? Matricule { get; set; }
}
