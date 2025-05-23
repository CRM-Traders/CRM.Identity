namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateSecretDeactivatedEvent : DomainEvent
{
    public Guid AffiliateId { get; }

    public AffiliateSecretDeactivatedEvent(Guid aggregateId, string aggregateType, Guid affiliateId)
        : base(aggregateId, aggregateType)
    {
        AffiliateId = affiliateId;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}