using CRM.Identity.Domain.Entities.Clients.DomainEvents;

namespace CRM.Identity.Domain.Entities.Clients;

public class Client : ClientBase
{
    public Affiliate.Affiliate? Affiliate { get; private set; }
    public Guid AffiliateId { get; private set; }

    private Client()
    {
    }

    public Client(
        string firstName,
        string lastName,
        string email,
        Guid affiliateId,
        string? telephone = null,
        string? country = null,
        string? language = null,
        DateTime? dateOfBirth = null,
        string? registrationIP = null,
        string? registrationSystem = null,
        string? registrationDevice = null,
        string? source = null)
        : base(firstName, lastName, email, telephone, country, language, dateOfBirth,
            registrationIP, registrationSystem, registrationDevice, source)
    {
        AffiliateId = affiliateId;

        AddDomainEvent(new ClientCreatedEvent(
            Id,
            GetType().Name,
            firstName,
            lastName,
            email,
            affiliateId,
            telephone,
            country,
            language,
            dateOfBirth,
            registrationIP,
            registrationSystem,
            registrationDevice,
            source));
    }

    public void AssignToAffiliate(Guid newAffiliateId)
    {
        var previousAffiliateId = AffiliateId;
        AffiliateId = newAffiliateId;
        Affiliate = null; // Will be loaded by EF
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientAssignedToAffiliateEvent(
            Id,
            GetType().Name,
            previousAffiliateId,
            newAffiliateId));
    }
}