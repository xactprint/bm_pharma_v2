namespace BMPharma.Domain.Common;

public abstract class DomainEvent : MediatR.INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}
