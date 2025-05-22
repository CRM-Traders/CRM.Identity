namespace CRM.Identity.Domain.Entities.Leads.DomainEvents;

public sealed class LeadCreatedEvent : DomainEvent
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Telephone { get; }
    public string? Country { get; }
    public string? Language { get; }
    public DateTime? DateOfBirth { get; }
    public string? RegistrationIP { get; }
    public string? RegistrationSystem { get; }
    public string? RegistrationDevice { get; }
    public string? Source { get; }

    public LeadCreatedEvent(
        Guid aggregateId,
        string aggregateType,
        string firstName,
        string lastName,
        string email,
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