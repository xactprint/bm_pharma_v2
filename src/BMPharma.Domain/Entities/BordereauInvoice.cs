using BMPharma.Domain.Common;

namespace BMPharma.Domain.Entities;

public class BordereauInvoice : BaseEntity
{
    public Guid BordereauId { get; set; }
    public Bordereau Bordereau { get; set; } = null!;
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}
