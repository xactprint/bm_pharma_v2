namespace BMPharma.CHIFA.Interfaces;

public interface IChifaTokenService
{
    Task<bool> IsTokenPresentAsync(CancellationToken cancellationToken = default);
    Task<TokenInfo?> GetTokenInfoAsync(CancellationToken cancellationToken = default);
}

public class TokenInfo
{
    public string? SerialNumber { get; set; }
    public string? Label { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsValid => ExpiryDate.HasValue && ExpiryDate.Value > DateTime.UtcNow;
}
