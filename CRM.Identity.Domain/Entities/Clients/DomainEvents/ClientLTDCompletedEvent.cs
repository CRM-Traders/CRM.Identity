namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientLTDCompletedEvent : DomainEvent
{
    public DateTime LTDTime { get; }

    public ClientLTDCompletedEvent(
        Guid aggregateId,
        string aggregateType,
        DateTime ltdTime) : base(aggregateId, aggregateType)
    {
        LTDTime = ltdTime;
        ProcessingStrategy = ProcessingStrategy.Background;
    }
}