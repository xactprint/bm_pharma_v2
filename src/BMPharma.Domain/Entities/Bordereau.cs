using BMPharma.Domain.Common;
using BMPharma.Domain.Enums;

namespace BMPharma.Domain.Entities;

public class Bordereau : AuditableEntity
{
    public string BordereauNumber { get; set; } = string.Empty;
    public DateTime BordereauDate { get; set; } = DateTime.UtcNow;
    public int TotalInvoices { get; set; }
    public decimal TotalAmountDA { get; set; }
    public BordereauStatus Status { get; set; } = BordereauStatus.Draft;
    public string? CnasType { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? SignatureId { get; set; }
    public string? RejectionReason { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<BordereauInvoice> BordereauInvoices { get; set; } = new List<BordereauInvoice>();

    public string? ChifaNumBord { get; set; }
    public ChifaWorkflowState ChifaState { get; set; } = ChifaWorkflowState.Draft;
    public ChifaIntegrationState ChifaIntegrationState { get; set; } = ChifaIntegrationState.Unknown;
    public string? ChifaErrorMessage { get; set; }
    public DateTime? ChifaLastUpdated { get; set; }
}
