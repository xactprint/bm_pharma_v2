using BMPharma.Domain.Common;

namespace BMPharma.Domain.Entities;

public class InvoiceLine : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPriceDA { get; set; }
    public decimal LineTotalDA { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string? Notes { get; set; }
    
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid? BatchId { get; set; }
    public Batch? Batch { get; set; }
}
