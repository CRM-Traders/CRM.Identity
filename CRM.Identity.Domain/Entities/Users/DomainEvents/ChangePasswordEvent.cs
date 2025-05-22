namespace CRM.Identity.Domain.Entities.Users.DomainEvents;

public sealed class ChangePasswordEvent : DomainEvent
{
    public string PasswordHash { get; } = string.Empty;
    public string PasswordSalt { get; } = string.Empty;

    public ChangePasswordEvent(
        Guid aggregateId,
        string aggregateType,
        string passwordHash,
        string salt) : base(aggregateId, aggregateType) 
    {
        PasswordHash = passwordHash;
        PasswordSalt = salt;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
