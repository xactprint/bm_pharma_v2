namespace BMPharma.CHIFA.Interfaces;

public interface IChifaBordereauService
{
    Task<ChifaBordereauResult> CreateBordereauAsync(ChifaBordereauRequest request, CancellationToken cancellationToken = default);
    Task<ChifaBordereauResult> SignBordereauAsync(string numBord, CancellationToken cancellationToken = default);
    Task<ChifaBordereauResult> CloseBordereauAsync(string numBord, CancellationToken cancellationToken = default);
    Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default);
}

public class ChifaBordereauRequest
{
    public string NumBord { get; set; } = string.Empty;
    public string TypeBord { get; set; } = "BORD_CNAS";
    public DateTime DateBord { get; set; } = DateTime.UtcNow;
    public List<string> InvoiceNumbers { get; set; } = new();
}

public class ChifaBordereauResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? NumBord { get; set; }
    public string? SignatureId { get; set; }
}
