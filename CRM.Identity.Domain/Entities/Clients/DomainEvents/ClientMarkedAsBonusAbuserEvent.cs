namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientMarkedAsBonusAbuserEvent : DomainEvent
{
    public string Reason { get; }

    public ClientMarkedAsBonusAbuserEvent(
        Guid aggregateId,
        string aggregateType,
        string reason) : base(aggregateId, aggregateType)
    {
        Reason = reason;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}