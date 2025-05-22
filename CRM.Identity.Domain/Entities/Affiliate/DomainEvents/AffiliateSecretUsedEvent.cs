namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateSecretUsedEvent : DomainEvent
{
    public Guid AffiliateId { get; }
    public int UsedCount { get; }

    public AffiliateSecretUsedEvent(
        Guid aggregateId,
        string aggregateType,
        Guid affiliateId,
        int usedCount) : base(aggregateId, aggregateType)
    {
        AffiliateId = affiliateId;
        UsedCount = usedCount;

        ProcessingStrategy = ProcessingStrategy.Background;
    }
}