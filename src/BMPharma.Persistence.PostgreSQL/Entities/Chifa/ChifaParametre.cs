using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

/// <summary>
/// Maps to the real CHIFA_OFFICINE parametre table (58 physical columns).
/// Singleton table — always 1 row. Keyless by design.
/// Schema: public | Source: BM-PHASE-004.9 real database discovery
/// </summary>
[Table("parametre")]
public class ChifaParametre
{
    [Column("code_ps")]
    public string? CodePs { get; set; }

    [Column("nom_pharmacie")]
    public string? NomPharmacie { get; set; }

    [Column("nom")]
    public string? Nom { get; set; }

    [Column("prenom")]
    public string? Prenom { get; set; }

    [Column("adresse")]
    public string? Adresse { get; set; }

    [Column("num_tel")]
    public string? NumTel { get; set; }

    [Column("num_fax")]
    public string? NumFax { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("code_sp")]
    public string? CodeSp { get; set; }

    [Column("nis")]
    public string? Nis { get; set; }

    [Column("nico")]
    public string? Nico { get; set; }

    [Column("ndps")]
    public string? Ndps { get; set; }

    [Column("code_centre")]
    public string? CodeCentre { get; set; }

    [Column("convention")]
    public string? Convention { get; set; }

    [Column("ref_convention")]
    public string? RefConvention { get; set; }

    [Column("ref_bancaire")]
    public string? RefBancaire { get; set; }

    [Column("mode_reglement")]
    public string? ModeReglement { get; set; }

    [Column("mont_max")]
    public decimal? MontMax { get; set; }

    [Column("contact")]
    public string? Contact { get; set; }

    [Column("mont_maj_fae")]
    public decimal? MontMajFae { get; set; }

    [Column("mont_maj_sub")]
    public decimal? MontMajSub { get; set; }

    [Column("taux_maj_local")]
    public decimal? TauxMajLocal { get; set; }

    [Column("taux_maj_inf_tr")]
    public decimal? TauxMajInfTr { get; set; }

    [Column("version")]
    public string? Version { get; set; }

    [Column("date_medicament")]
    public DateTime? DateMedicament { get; set; }

    [Column("date_liste_noire")]
    public DateTime? DateListeNoire { get; set; }

    [Column("date_liste_mc")]
    public DateTime? DateListeMc { get; set; }

    [Column("date_version")]
    public DateTime? DateVersion { get; set; }

    [Column("date_specialite")]
    public DateTime? DateSpecialite { get; set; }

    [Column("date_tarif")]
    public DateTime? DateTarif { get; set; }

    [Column("date_note")]
    public DateTime? DateNote { get; set; }

    [Column("date_convention")]
    public string? DateConvention { get; set; }

    [Column("nb_ord_max")]
    public int? NbOrdMax { get; set; }

    [Column("officine_dgsn")]
    public bool? OfficineDgsn { get; set; }

    [Column("chemin_backup")]
    public string? CheminBackup { get; set; }

    [Column("heure_backup")]
    public string? HeureBackup { get; set; }

    [Column("nb_backup")]
    public int? NbBackup { get; set; }

    [Column("next_num_fact")]
    public int? NextNumFact { get; set; }

    [Column("next_num_bord")]
    public short? NextNumBord { get; set; }

    [Column("poste_serveur_chifa")]
    public bool? PosteServeurChifa { get; set; }

    [Column("poste_telech")]
    public string? PosteTelech { get; set; }

    [Column("date_api_chifa")]
    public DateTime? DateApiChifa { get; set; }

    [Column("date_verif_maj")]
    public DateTime? DateVerifMaj { get; set; }

    [Column("date_verif_cm")]
    public DateTime? DateVerifCm { get; set; }

    [Column("date_mut_radie")]
    public DateTime? DateMutRadie { get; set; }

    [Column("params")]
    public string? Params { get; set; }

    [Column("version_db")]
    public int? VersionDb { get; set; }

    [Column("version_ftp")]
    public int? VersionFtp { get; set; }

    [Column("date_version_ftp")]
    public DateTime? DateVersionFtp { get; set; }

    [Column("annee")]
    public int? Annee { get; set; }

    [Column("date_ln_complete")]
    public DateTime? DateLnComplete { get; set; }

    [Column("backup_start")]
    public bool? BackupStart { get; set; }

    [Column("backup_exit")]
    public bool? BackupExit { get; set; }

    [Column("date_medicament2")]
    public DateTime? DateMedicament2 { get; set; }

    [Column("date_medic_ppa")]
    public DateTime? DateMedicPpa { get; set; }

    [Column("access_token")]
    public string? AccessToken { get; set; }

    [Column("refresh_token")]
    public string? RefreshToken { get; set; }

    [Column("date_medic_demuni")]
    public DateTime? DateMedicDemuni { get; set; }
}
