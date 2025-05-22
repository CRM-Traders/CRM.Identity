namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientMarkedAsProblematicEvent : DomainEvent
{
    public string? Reason { get; }

    public ClientMarkedAsProblematicEvent(
        Guid aggregateId,
        string aggregateType,
        string? reason) : base(aggregateId, aggregateType)
    {
        Reason = reason;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}