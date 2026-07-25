namespace BMPharma.CNAS.Interfaces;

public interface ICnasService
{
    Task<string> GetNextBordereauNumberAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateInsuredPatientAsync(string numAssure, CancellationToken cancellationToken = default);
}
