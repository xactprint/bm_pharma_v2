namespace BMPharma.Domain.Enums;

public enum ChifaOperationStatus
{
    NotStarted = 0,
    InProgress = 1,
    DatabaseCreated = 2,
    CHIFAVisible = 3,
    ReadyForSigning = 4,
    SigningInProgress = 5,
    Signed = 6,
    Closed = 7,
    Transmitted = 8,
    Rejected = 9,
    Failed = 10,
    SimulatedDatabaseCreated = 11,
    SimulatedCHIFAVisible = 12,
    SimulatedSigned = 13,
    SimulatedClosed = 14,
    SimulatedTransmitted = 15
}
