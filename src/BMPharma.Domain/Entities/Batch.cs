using BMPharma.Domain.Common;

namespace BMPharma.Domain.Entities;

public class Batch : BaseEntity
{
    public string LotNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public int? QuantityReserved { get; set; }
    public decimal UnitPriceDA { get; set; }
    public string? Supplier { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public bool IsExpired => ExpiryDate < DateTime.UtcNow;
    public bool IsLowStock => (Quantity - (QuantityReserved ?? 0)) <= 0;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
