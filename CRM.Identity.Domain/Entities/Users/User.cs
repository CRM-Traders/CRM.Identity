namespace CRM.Identity.Domain.Entities.Users;

public class User : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; } = false;
    public Role Role { get; private set; } = Role.User;

    public User(string firstName, string lastName, string email, string? phoneNumber, string passwordHash, string passwordSalt)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;

        AddDomainEvent(new UserCreatedEvent(
            Id,
            GetType().Name,
            firstName,
            lastName,
            email,
            phoneNumber,
            passwordHash,
            passwordSalt));
    }
}
