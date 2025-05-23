namespace CRM.Identity.Domain.Entities.Users;

public class User : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; } = false;
    public Role Role { get; private set; } = Role.User;

    public bool IsTwoFactorEnabled { get; private set; } = false;
    public string? TwoFactorSecret { get; private set; }
    public bool IsTwoFactorVerified { get; private set; } = false;
    public List<string> RecoveryCodes { get; private set; } = new();
    
    
    public User(string firstName, string lastName, string email, string username, string? phoneNumber, string passwordHash, string passwordSalt)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;

        AddDomainEvent(new UserCreatedEvent(
            Id,
            GetType().Name,
            firstName,
            lastName,
            email,
            username,
            phoneNumber,
            passwordHash,
            passwordSalt));
    }
     
    public void EnableTwoFactorAuthentication(string secret)
    {
        TwoFactorSecret = secret;
        IsTwoFactorEnabled = true;
        IsTwoFactorVerified = false;
        
        AddDomainEvent(new TwoFactorEnabledEvent(Id, GetType().Name));
    }
     
    public void VerifyTwoFactorAuthentication(List<string> recoveryCodes)
    {
        IsTwoFactorVerified = true;
        RecoveryCodes = recoveryCodes;
        
        AddDomainEvent(new TwoFactorVerifiedEvent(Id, GetType().Name));
    }
     
    public void DisableTwoFactorAuthentication()
    {
        IsTwoFactorEnabled = false;
        IsTwoFactorVerified = false;
        TwoFactorSecret = null;
        RecoveryCodes.Clear();
        
        AddDomainEvent(new TwoFactorDisabledEvent(Id, GetType().Name));
    }
     
    public bool UseRecoveryCode(string code)
    {
        if (RecoveryCodes.Contains(code))
        {
            RecoveryCodes.Remove(code);
            AddDomainEvent(new RecoveryCodeUsedEvent(Id, GetType().Name, code));
            return true;
        }
        return false;
    }

    public void ChangePassword(string passwordHash, string passwordSalt) 
    {
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;

        AddDomainEvent(new ChangePasswordEvent(Id, GetType().Name, passwordHash, passwordSalt));
    }
}
