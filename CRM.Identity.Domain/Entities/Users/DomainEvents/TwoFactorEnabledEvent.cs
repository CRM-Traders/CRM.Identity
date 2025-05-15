namespace CRM.Identity.Domain.Entities.Users.DomainEvents;

public sealed class TwoFactorEnabledEvent : DomainEvent
{
    public TwoFactorEnabledEvent(Guid aggregateId, string aggregateType) 
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
