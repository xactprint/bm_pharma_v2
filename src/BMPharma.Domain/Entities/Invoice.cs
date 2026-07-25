using BMPharma.Domain.Common;
using BMPharma.Domain.Enums;

namespace BMPharma.Domain.Entities;

public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotalDA { get; set; }
    public decimal TaxDA { get; set; }
    public decimal TotalDA { get; set; }
    public decimal? DiscountDA { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
