namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientFTDCompletedEvent : DomainEvent
{
    public DateTime FTDTime { get; }

    public ClientFTDCompletedEvent(
        Guid aggregateId,
        string aggregateType,
        DateTime ftdTime) : base(aggregateId, aggregateType)
    {
        FTDTime = ftdTime;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}