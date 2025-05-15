namespace CRM.Identity.Domain.Entities.Users.DomainEvents;

public sealed class TwoFactorVerifiedEvent : DomainEvent
{
    public TwoFactorVerifiedEvent(Guid aggregateId, string aggregateType) 
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
