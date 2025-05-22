namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientUnmarkedAsBonusAbuserEvent : DomainEvent
{
    public ClientUnmarkedAsBonusAbuserEvent(Guid aggregateId, string aggregateType)
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}