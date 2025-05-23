using CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

namespace CRM.Identity.Domain.Entities.Affiliate;

public class Affiliate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Affiliate()
    {
    }

    public Affiliate(string name, string email, string? phone = null, string? website = null)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Website = website;
        IsActive = true;

        AddDomainEvent(new AffiliateCreatedEvent(
            Id,
            GetType().Name,
            name,
            email,
            phone,
            website));
    }

    public void UpdateDetails(string name, string email, string? phone = null, string? website = null)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Website = website;

        AddDomainEvent(new AffiliateDetailsUpdatedEvent(
            Id,
            GetType().Name,
            name,
            email,
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