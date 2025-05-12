namespace CRM.Identity.Domain.Common.Events;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTimeOffset OccurredOn { get; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }
}