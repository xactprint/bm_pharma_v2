using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

/// <summary>
/// Maps to the real CHIFA_OFFICINE facture table (53 physical columns).
/// 7 columns were dropped by CHIFA migrations (attnum gaps 34-35, 40, 42-45).
/// Schema: public | Source: BM-PHASE-004.9 real database discovery
/// </summary>
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

    [Column("rang_ad")]
    public string? RangAd { get; set; }

    [Column("code_centre")]
    public string? CodeCentre { get; set; }

    [Column("tp")]
    public string? Tp { get; set; }

    [Column("taux")]
    public string? Taux { get; set; }

    [Column("code_affect")]
    public string? CodeAffect { get; set; }

    [Column("conv")]
    public string? Conv { get; set; }

    [Column("type_consult")]
    public string? TypeConsult { get; set; }

    [Column("prescripteur")]
    public string? Prescripteur { get; set; }

    [Column("date_soin")]
    public DateTime? DateSoin { get; set; }

    [Column("risque")]
    public string? Risque { get; set; }

    [Column("statut_fact")]
    public string? StatutFact { get; set; }

    [Column("verifcms")]
    public string? Verifcms { get; set; }

    [Column("type_signature")]
    public string? TypeSignature { get; set; }

    [Column("verif_fact")]
    public string? VerifFact { get; set; }

    [Column("mont_maj_fae")]
    public decimal MontMajFae { get; set; }

    [Column("mont_maj")]
    public decimal MontMaj { get; set; }

    [Column("type_maj")]
    public int TypeMaj { get; set; }

    [Column("code_centre_as")]
    public string? CodeCentreAs { get; set; }

    [Column("code_sp")]
    public string? CodeSp { get; set; }

    [Column("type_ord")]
    public string? TypeOrd { get; set; }

    [Column("motif_med")]
    public string? MotifMed { get; set; }

    [Column("id_user")]
    public int? IdUser { get; set; }

    [Column("signature")]
    public string? Signature { get; set; }

    [Column("num_serie")]
    public long? NumSerie { get; set; }

    [Column("date_envoi_sms")]
    public DateTime? DateEnvoiSms { get; set; }

    [Column("date_fin_droit")]
    public DateTime? DateFinDroit { get; set; }

    [Column("date_fin_droit_benef")]
    public DateTime? DateFinDroitBenef { get; set; }

    [Column("version")]
    public string? Version { get; set; }

    [Column("code_covid")]
    public string? CodeCovid { get; set; }

    [Column("fact_xml")]
    public string? FactXml { get; set; }

    [Column("nat_remb")]
    public string? NatRemb { get; set; }

    [Column("mont_mut")]
    public decimal? MontMut { get; set; }

    [Column("date_fin_mut")]
    public DateTime? DateFinMut { get; set; }

    [Column("code_mut")]
    public string? CodeMut { get; set; }

    [Column("date_synchro")]
    public DateTime? DateSynchro { get; set; }

    [Column("adresse_ip")]
    public string? AdresseIp { get; set; }

    [Column("nom_pc")]
    public string? NomPc { get; set; }

    [Column("obs")]
    public string? Obs { get; set; }

    [Column("ref_cm")]
    public string? RefCm { get; set; }

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
}
