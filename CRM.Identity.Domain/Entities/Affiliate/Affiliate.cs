using CRM.Identity.Domain.Entities.Affiliate.DomainEvents;
using CRM.Identity.Domain.Entities.Users;

namespace CRM.Identity.Domain.Entities.Affiliate;

public class Affiliate : AggregateRoot
{ 
    public string? Phone { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid UserId { get; set; }
    public User? User { get; set; }

    private Affiliate()
    {
    }

    public Affiliate( string? phone = null, string? website = null)
    { 
        Phone = phone;
        Website = website;
        IsActive = true;

        AddDomainEvent(new AffiliateCreatedEvent(
            Id,
            GetType().Name, 
            phone,
            website));
    }

    public void UpdateDetails(string? phone = null, string? website = null)
    { 
        Phone = phone;
        Website = website;

        AddDomainEvent(new AffiliateDetailsUpdatedEvent(
            Id,
            GetType().Name, 
            phone,
            website));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(new AffiliateActivatedEvent(Id, GetType().Name));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(new AffiliateDeactivatedEvent(Id, GetType().Name));
    }
}