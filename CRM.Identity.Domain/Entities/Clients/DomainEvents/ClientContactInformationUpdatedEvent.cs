namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientContactInformationUpdatedEvent : DomainEvent
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Telephone { get; }
    public string? SecondTelephone { get; }
    public string? Skype { get; }

    public ClientContactInformationUpdatedEvent(
        Guid aggregateId,
        string aggregateType,
        string firstName,
        string lastName,
        string email,
        string? telephone,
        string? secondTelephone,
        string? skype) : base(aggregateId, aggregateType)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Telephone = telephone;
        SecondTelephone = secondTelephone;
        Skype = skype;

        ProcessingStrategy = ProcessingStrategy.Background;
    }
}