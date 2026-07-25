using BMPharma.Domain.Common;
using BMPharma.Domain.Enums;

namespace BMPharma.Domain.Entities;

public class License : BaseEntity
{
    public string LicenseKey { get; set; } = string.Empty;
    public string MachineFingerprint { get; set; } = string.Empty;
    public AppMode Mode { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsMaintenanceIncluded { get; set; }
    public DateTime? MaintenanceExpiryDate { get; set; }
    public string? licenseeName { get; set; }
    public string? LicenseeEmail { get; set; }
    public DateTime ActivatedAt { get; set; }
    public bool IsValid => Mode == AppMode.Permanent || 
                           (ExpiryDate.HasValue && ExpiryDate.Value > DateTime.UtcNow);
}
