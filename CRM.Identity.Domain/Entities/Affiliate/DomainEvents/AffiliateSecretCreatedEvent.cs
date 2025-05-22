namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateSecretCreatedEvent : DomainEvent
{
    public Guid AffiliateId { get; }
    public string SecretKey { get; }
    public string ApiKey { get; }
    public DateTimeOffset ExpirationDate { get; }
    public string? IpRestriction { get; }

    public AffiliateSecretCreatedEvent(
        Guid aggregateId,
        string aggregateType,
        Guid affiliateId,
        string secretKey,
        string apiKey,
        DateTimeOffset expirationDate,
        string? ipRestriction) : base(aggregateId, aggregateType)
    {
        AffiliateId = affiliateId;
        SecretKey = secretKey;
        ApiKey = apiKey;
        ExpirationDate = expirationDate;
        IpRestriction = ipRestriction;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}