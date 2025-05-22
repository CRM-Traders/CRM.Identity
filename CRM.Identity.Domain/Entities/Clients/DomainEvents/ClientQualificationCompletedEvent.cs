namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientQualificationCompletedEvent : DomainEvent
{
    public DateTime QualificationTime { get; }

    public ClientQualificationCompletedEvent(
        Guid aggregateId,
        string aggregateType,
        DateTime qualificationTime) : base(aggregateId, aggregateType)
    {
        QualificationTime = qualificationTime;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}