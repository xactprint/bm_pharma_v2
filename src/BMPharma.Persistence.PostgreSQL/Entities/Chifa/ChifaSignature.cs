using System.ComponentModel.DataAnnotations.Schema;

namespace BMPharma.Persistence.PostgreSQL.Entities.Chifa;

/// <summary>
/// Maps to the real CHIFA_OFFICINE signature table (2 columns).
/// Stores digital signatures for invoices. Empty table (0 rows, 181 MB pre-allocated).
/// Schema: public | Source: BM-PHASE-004.9 real database discovery
/// </summary>
[Table("signature")]
public class ChifaSignature
{
    [Column("num_fact")]
    public string NumFact { get; set; } = null!;

    [Column("sign")]
    public string? Sign { get; set; }
}
