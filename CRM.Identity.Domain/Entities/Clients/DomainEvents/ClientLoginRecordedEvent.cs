namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientLoginRecordedEvent : DomainEvent
{
    public DateTime LoginTime { get; }

    public ClientLoginRecordedEvent(
        Guid aggregateId,
        string aggregateType,
        DateTime loginTime) : base(aggregateId, aggregateType)
    {
        LoginTime = loginTime;
        ProcessingStrategy = ProcessingStrategy.Background;
    }
}