namespace CRM.Identity.Domain.Common.Events;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public ProcessingStrategy ProcessingStrategy { get; protected set; } =
        ProcessingStrategy.Background;

    protected DomainEvent(Guid aggregateId, string aggregateType)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
    }
}

public enum ProcessingStrategy
{
    Immediate,
    Background
}