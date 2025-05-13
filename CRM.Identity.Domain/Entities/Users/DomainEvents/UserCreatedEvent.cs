namespace CRM.Identity.Domain.Entities.Users.DomainEvents;

public sealed class UserCreatedEvent : DomainEvent
{
    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public string Email { get; } = string.Empty;
    public string? PhoneNumber { get; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;

    public UserCreatedEvent(
        Guid aggregateId,
        string aggregateType,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        string passwordHash,
        string passwordSalt) : base(aggregateId, aggregateType)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
