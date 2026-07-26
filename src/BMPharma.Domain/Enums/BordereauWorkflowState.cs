namespace BMPharma.Domain.Enums;

public enum BordereauWorkflowState
{
    Draft = 0,
    Preparing = 1,
    Created = 2,
    InvoicesAttached = 3,
    AwaitingSignature = 4,
    PartiallySigned = 5,
    ReadyForClosure = 6,
    AwaitingClosure = 7,
    Closed = 8,
    AwaitingTransmission = 9,
    Transmitted = 10,
    Completed = 11,
    Error = 50,
    SyncError = 51,
    SignatureError = 52,
    ClosureError = 53,
    TransmissionError = 54
}
