namespace BMPharma.Domain.Enums;

public enum ChifaWorkflowState
{
    Draft = 0,
    Validated = 1,
    PreparedForChifa = 2,
    WrittenToChifa = 3,
    VisibleInChifa = 4,
    Signed = 5,
    BordereauAssigned = 6,
    BordereauClosed = 7,
    Transmitted = 8,
    Failed = 9,
    Rejected = 10,
    RollbackRequired = 11,
    Cancelled = 12
}
