using BMPharma.Domain.Common;
using BMPharma.Domain.Enums;

namespace BMPharma.Domain.Entities;

public class Payment : BaseEntity
{
    public decimal AmountDA { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}
