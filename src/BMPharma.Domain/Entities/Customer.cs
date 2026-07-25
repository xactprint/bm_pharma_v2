using BMPharma.Domain.Common;

namespace BMPharma.Domain.Entities;

public class Customer : AuditableEntity
{
    public string? NationalId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public bool IsInsured { get; set; }
    public string? InsuranceNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
