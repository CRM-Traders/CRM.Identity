namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateDeactivatedEvent : DomainEvent
{
    public AffiliateDeactivatedEvent(Guid aggregateId, string aggregateType)
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
