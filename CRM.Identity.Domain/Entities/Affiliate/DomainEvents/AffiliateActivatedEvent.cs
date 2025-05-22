namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateActivatedEvent : DomainEvent
{
    public AffiliateActivatedEvent(Guid aggregateId, string aggregateType)
        : base(aggregateId, aggregateType)
    {
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}