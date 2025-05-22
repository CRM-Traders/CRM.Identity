using CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

namespace CRM.Identity.Domain.Entities.Affiliate;

public class AffiliateSecret : AggregateRoot
{
    public Guid AffiliateId { get; private set; }
    public Affiliate? Affiliate { get; set; }
    public string SecretKey { get; private set; } = string.Empty;
    public string ApiKey { get; private set; } = string.Empty;
    public DateTimeOffset ExpirationDate { get; private set; }
    public string? IpRestriction { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int UsedCount { get; private set; } = 0;

    private AffiliateSecret()
    {
    }

    public AffiliateSecret(
        Guid affiliateId,
        string secretKey,
        string apiKey,
        DateTimeOffset expirationDate,
        string? ipRestriction = null)
    {
        AffiliateId = affiliateId;
        SecretKey = secretKey;
        ApiKey = apiKey;
        ExpirationDate = expirationDate;
        IpRestriction = ipRestriction;
        IsActive = true;
        UsedCount = 0;

        AddDomainEvent(new AffiliateSecretCreatedEvent(
            Id,
            GetType().Name,
            affiliateId,
            secretKey,
            apiKey,
            expirationDate,
            ipRestriction));
    }

    public bool TryUse()
    {
        if (!IsActive || IsExpired())
        {
            return false;
        }

        UsedCount++;
        AddDomainEvent(new AffiliateSecretUsedEvent(
            Id,
            GetType().Name,
            AffiliateId,
            UsedCount));

        return true;
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(new AffiliateSecretActivatedEvent(Id, GetType().Name, AffiliateId));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(new AffiliateSecretDeactivatedEvent(Id, GetType().Name, AffiliateId));
    }

    public void UpdateExpiration(DateTimeOffset newExpirationDate)
    {
        ExpirationDate = newExpirationDate;
        AddDomainEvent(new AffiliateSecretExpirationUpdatedEvent(
            Id,
            GetType().Name,
            AffiliateId,
            newExpirationDate));
    }

    public bool IsExpired() => DateTimeOffset.UtcNow > ExpirationDate;
}