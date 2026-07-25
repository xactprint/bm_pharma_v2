using BMPharma.Domain.Common;
using BMPharma.Domain.Enums;

namespace BMPharma.Domain.Entities;

public class StockMovement : BaseEntity
{
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? BatchId { get; set; }
    public Batch? Batch { get; set; }
}
