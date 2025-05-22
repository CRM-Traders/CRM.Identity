using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads.DomainEvents;

namespace CRM.Identity.Domain.Entities.Leads;

public class Lead : ClientBase
{
    private Lead()
    {
    }

    public Lead(
        string firstName,
        string lastName,
        string email,
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
        AddDomainEvent(new LeadCreatedEvent(
            Id,
            GetType().Name,
            firstName,
            lastName,
            email,
            telephone,
            country,
            language,
            dateOfBirth,
            registrationIP,
            registrationSystem,
            registrationDevice,
            source));
    }

    public Client ConvertToClient(Guid affiliateId)
    {
        var client = new Client(
            FirstName,
            LastName,
            Email,
            affiliateId,
            Telephone,
            Country,
            Language,
            DateOfBirth,
            RegistrationIP,
            RegistrationSystem,
            RegistrationDevice,
            Source);

        AddDomainEvent(new LeadConvertedToClientEvent(
            Id,
            GetType().Name,
            client.Id,
            affiliateId));

        return client;
    }
}