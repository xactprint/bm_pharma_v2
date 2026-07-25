using BMPharma.Domain.Common;
using BMPharma.Domain.Enums;

namespace BMPharma.Domain.Entities;

public class Product : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameFr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductCategory Category { get; set; }
    public string? Manufacturer { get; set; }
    public string? ManufacturerAr { get; set; }
    public string? ActiveIngredient { get; set; }
    public string? Dosage { get; set; }
    public string? PharmaceuticalForm { get; set; }
    public string? Packaging { get; set; }
    public string? CIPCode { get; set; }
    public string? EANCode { get; set; }
    public decimal PriceDA { get; set; }
    public decimal? PurchasePriceDA { get; set; }
    public bool IsReimbursable { get; set; }
    public decimal? ReimbursementRate { get; set; }
    public bool IsActive { get; set; } = true;
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    
    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
