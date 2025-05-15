namespace CRM.Identity.Domain.Entities.Users.DomainEvents;

public sealed class TwoFactorDisabledEvent : DomainEvent
{
    public TwoFactorDisabledEvent(Guid aggregateId, string aggregateType) 
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
