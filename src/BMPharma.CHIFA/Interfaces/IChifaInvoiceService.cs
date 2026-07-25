namespace BMPharma.CHIFA.Interfaces;

public interface IChifaInvoiceService
{
    Task<ChifaInvoiceResult> CreateInvoiceAsync(ChifaInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<bool> InvoiceExistsInChifaAsync(string numFact, CancellationToken cancellationToken = default);
}

public class ChifaInvoiceRequest
{
    public string NumFact { get; set; } = string.Empty;
    public string NumAssure { get; set; } = string.Empty;
    public int CodeCentre { get; set; }
    public DateTime DateSoin { get; set; }
    public List<ChifaInvoiceLineRequest> Lines { get; set; } = new();
}

public class ChifaInvoiceLineRequest
{
    public string NumEnr { get; set; } = string.Empty;
    public int MedicCode { get; set; }
    public decimal PrixUnit { get; set; }
    public int Quantite { get; set; }
    public int InfTr { get; set; } = 1;
    public int ApplicTr { get; set; } = 1;
    public int Medic { get; set; } = 1;
    public int Ts { get; set; } = 4;
    public int DureeTrait { get; set; } = 5;
}

public class ChifaInvoiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ChifaNumFact { get; set; }
}
