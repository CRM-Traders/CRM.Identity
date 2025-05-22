namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateSecretExpirationUpdatedEvent : DomainEvent
{
    public Guid AffiliateId { get; }
    public DateTimeOffset NewExpirationDate { get; }

    public AffiliateSecretExpirationUpdatedEvent(
        Guid aggregateId,
        string aggregateType,
        Guid affiliateId,
        DateTimeOffset newExpirationDate) : base(aggregateId, aggregateType)
    {
        AffiliateId = affiliateId;
        NewExpirationDate = newExpirationDate;

        ProcessingStrategy = ProcessingStrategy.Background;
    }
}