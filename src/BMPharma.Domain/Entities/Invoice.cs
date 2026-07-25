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

    public ChifaWorkflowState ChifaState { get; set; } = ChifaWorkflowState.Draft;
    public ChifaIntegrationState ChifaIntegrationState { get; set; } = ChifaIntegrationState.Unknown;
    public string? ChifaNumFact { get; set; }
    public string? ChifaNumAssure { get; set; }
    public int? ChifaCodeCentre { get; set; }
    public decimal ChifaMontFact { get; set; }
    public decimal ChifaMontAs { get; set; }
    public decimal ChifaMontMut { get; set; }
    public DateTime? ChifaDateFact { get; set; }
    public DateTime? ChifaDateFinMut { get; set; }
    public string? ChifaNumBord { get; set; }
    public string? ChifaErrorMessage { get; set; }
    public DateTime? ChifaLastUpdated { get; set; }
}
