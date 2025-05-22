namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientCommunicationRecordedEvent : DomainEvent
{
    public DateTime CommunicationTime { get; }

    public ClientCommunicationRecordedEvent(
        Guid aggregateId,
        string aggregateType,
        DateTime communicationTime) : base(aggregateId, aggregateType)
    {
        CommunicationTime = communicationTime;
        ProcessingStrategy = ProcessingStrategy.Background;
    }
}