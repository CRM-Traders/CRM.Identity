namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateSecretActivatedEvent : DomainEvent
{
    public Guid AffiliateId { get; }

    public AffiliateSecretActivatedEvent(Guid aggregateId, string aggregateType, Guid affiliateId)
        : base(aggregateId, aggregateType)
    {
        AffiliateId = affiliateId;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}