namespace BMPharma.CHIFA.Interfaces;

public interface IChifaSigningService
{
    Task<ChifaSigningStatus> GetSigningStatusAsync(string bordereauNumber, CancellationToken cancellationToken = default);
    Task<bool> IsTokenAvailableAsync(CancellationToken cancellationToken = default);
}

public enum ChifaSigningStatus
{
    NotSigned = 0,
    SigningRequired = 1,
    SigningInProgress = 2,
    Signed = 3,
    SigningFailed = 4,
    TokenNotPresent = 5
}
