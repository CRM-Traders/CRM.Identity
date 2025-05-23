namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;


public sealed class ClientCreatedEvent : DomainEvent
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public Guid AffiliateId { get; }
    public string? Telephone { get; }
    public string? Country { get; }
    public string? Language { get; }
    public DateTime? DateOfBirth { get; }
    public string? RegistrationIP { get; }
    public string? RegistrationSystem { get; }
    public string? RegistrationDevice { get; }
    public string? Source { get; }

    public ClientCreatedEvent(
        Guid aggregateId,
        string aggregateType,
        string firstName,
        string lastName,
        string email,
        Guid affiliateId,
        string? telephone,
        string? country,
        string? language,
        DateTime? dateOfBirth,
        string? registrationIP,
        string? registrationSystem,
        string? registrationDevice,
        string? source) : base(aggregateId, aggregateType)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        AffiliateId = affiliateId;
        Telephone = telephone;
        Country = country;
        Language = language;
        DateOfBirth = dateOfBirth;
        RegistrationIP = registrationIP;
        RegistrationSystem = registrationSystem;
        RegistrationDevice = registrationDevice;
        Source = source;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}