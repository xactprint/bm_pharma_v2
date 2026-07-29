namespace BMPharma.CHIFA.Services;

public enum TechnicalStatus
{
    Unknown,
    Connected,
    Degraded,
    Disconnected
}

public enum BusinessStatus
{
    Unknown,
    Draft,
    Prepared,
    Validated,
    Persisted,
    Failed
}

public enum VisibilityStatus
{
    Unknown,
    VisibleInFacture,
    VisibleInBordereau,
    NotVisible
}

public class ChifaStatusSnapshot
{
    public TechnicalStatus Technical { get; set; } = TechnicalStatus.Unknown;
    public BusinessStatus Business { get; set; } = BusinessStatus.Unknown;
    public VisibilityStatus Visibility { get; set; } = VisibilityStatus.Unknown;
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; }

    public bool IsReady =>
        Technical == TechnicalStatus.Connected &&
        Business == BusinessStatus.Persisted &&
        Visibility == VisibilityStatus.VisibleInFacture;
}

public class ChifaInvoiceStatusSnapshot
{
    public string NumFact { get; set; } = string.Empty;
    public BusinessStatus Business { get; set; } = BusinessStatus.Unknown;
    public VisibilityStatus Visibility { get; set; } = VisibilityStatus.Unknown;
    public string? Etat { get; set; }
    public string? NumBord { get; set; }
    public decimal MontFact { get; set; }
    public bool ExistsInChifa { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ChifaBordereauStatusSnapshot
{
    public string NumBord { get; set; } = string.Empty;
    public string? Etat { get; set; }
    public bool ExistsInChifa { get; set; }
    public int InvoiceCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ChifaSyncSummary
{
    public int InvoicesFound { get; set; }
    public int InvoicesUpdated { get; set; }
    public int BordereauxFound { get; set; }
    public int BordereauxUpdated { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; }
}
