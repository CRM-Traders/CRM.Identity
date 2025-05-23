namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientUnmarkedAsProblematicEvent : DomainEvent
{
    public ClientUnmarkedAsProblematicEvent(Guid aggregateId, string aggregateType)
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}