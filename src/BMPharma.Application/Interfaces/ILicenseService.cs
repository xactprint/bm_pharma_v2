using BMPharma.Domain.Enums;

namespace BMPharma.Application.Interfaces;

public interface ILicenseService
{
    Task<AppMode> GetCurrentModeAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateLicenseAsync(CancellationToken cancellationToken = default);
    Task<bool> ActivateLicenseAsync(string licenseKey, CancellationToken cancellationToken = default);
    Task<int> GetRemainingTrialDaysAsync(CancellationToken cancellationToken = default);
}
