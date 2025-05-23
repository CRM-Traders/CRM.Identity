using CRM.Identity.Domain.Entities.Clients.DomainEvents;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Domain.Entities.Clients;

public class ClientBase : AggregateRoot
{
    public string FirstName { get; protected set; } = string.Empty;
    public string LastName { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;
    public string? Telephone { get; protected set; }
    public string? SecondTelephone { get; protected set; }
    public string? Skype { get; protected set; }
    public string? Country { get; protected set; }
    public string? Language { get; protected set; }
    public DateTime? DateOfBirth { get; protected set; }

    public ClientStatus Status { get; protected set; } = ClientStatus.Active;
    public string? KycStatusId { get; protected set; }
    public string? SalesStatus { get; protected set; }
    public bool IsProblematic { get; protected set; } = false;
    public bool IsBonusAbuser { get; protected set; } = false;
    public string? BonusAbuserReason { get; protected set; }
    public bool HasInvestments { get; protected set; } = false;
    public string? SecurityCode { get; protected set; }
    public string? ClientArea { get; protected set; }

    public string? IDPassportNumber { get; protected set; }
    public string? CountrySpecificIdentifier { get; protected set; }
    public string? MarketingType { get; protected set; }
    public string? RegistrationNotes { get; protected set; }

    public DateTime RegistrationDate { get; protected set; }
    public string? RegistrationIP { get; protected set; }
    public string? RegistrationSystem { get; protected set; }
    public string? RegistrationDevice { get; protected set; }
    public DateTime? LastLogin { get; protected set; }
    public DateTime? LastCommunication { get; protected set; }
    public DateTime? LastUpdate { get; protected set; }
    public string? Source { get; protected set; }
    public string? RefererType { get; protected set; }

    public DateTime? FTDTime { get; protected set; }
    public DateTime? LTDTime { get; protected set; }
    public DateTime? QualificationTime { get; protected set; }
    public bool AllowTransactions { get; protected set; } = true;

    public bool AnonymousCall { get; protected set; } = false;
    public virtual ICollection<TradingAccount>? TradingAccounts { get; protected set; }

    protected ClientBase() { }

    protected ClientBase(
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
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Telephone = telephone;
        Country = country;
        Language = language;
        DateOfBirth = dateOfBirth;
        Status = ClientStatus.Active;
        RegistrationDate = DateTime.UtcNow;
        RegistrationIP = registrationIP;
        RegistrationSystem = registrationSystem;
        RegistrationDevice = registrationDevice;
        Source = source;
        LastUpdate = DateTime.UtcNow;
        TradingAccounts = new List<TradingAccount>();
    }

    public void UpdateContactInformation(
        string firstName,
        string lastName,
        string email,
        string? telephone = null,
        string? secondTelephone = null,
        string? skype = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Telephone = telephone;
        SecondTelephone = secondTelephone;
        Skype = skype;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientContactInformationUpdatedEvent(
            Id,
            GetType().Name,
            firstName,
            lastName,
            email,
            telephone,
            secondTelephone,
            skype));
    }

    public void ChangeStatus(ClientStatus newStatus)
    {
        if (Status == newStatus) return;

        var previousStatus = Status;
        Status = newStatus;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientStatusChangedEvent(
            Id,
            GetType().Name,
            previousStatus,
            newStatus));
    }

    public void UpdateKycStatus(string kycStatusId)
    {
        var previousKycStatus = KycStatusId;
        KycStatusId = kycStatusId;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientKycStatusChangedEvent(
            Id,
            GetType().Name,
            previousKycStatus,
            kycStatusId));
    }

    public void MarkAsProblematic(string? reason = null)
    {
        if (IsProblematic) return;

        IsProblematic = true;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientMarkedAsProblematicEvent(
            Id,
            GetType().Name,
            reason));
    }

    public void UnmarkAsProblematic()
    {
        if (!IsProblematic) return;

        IsProblematic = false;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientUnmarkedAsProblematicEvent(Id, GetType().Name));
    }

    public void MarkAsBonusAbuser(string reason)
    {
        if (IsBonusAbuser) return;

        IsBonusAbuser = true;
        BonusAbuserReason = reason;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientMarkedAsBonusAbuserEvent(
            Id,
            GetType().Name,
            reason));
    }

    public void UnmarkAsBonusAbuser()
    {
        if (!IsBonusAbuser) return;

        IsBonusAbuser = false;
        BonusAbuserReason = null;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientUnmarkedAsBonusAbuserEvent(Id, GetType().Name));
    }

    public void RecordFTD()
    {
        if (FTDTime.HasValue) return;

        FTDTime = DateTime.UtcNow;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientFTDCompletedEvent(
            Id,
            GetType().Name,
            FTDTime.Value));
    }

    public void RecordLTD()
    {
        LTDTime = DateTime.UtcNow;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientLTDCompletedEvent(
            Id,
            GetType().Name,
            LTDTime.Value));
    }

    public void RecordQualification()
    {
        if (QualificationTime.HasValue) return;

        QualificationTime = DateTime.UtcNow;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientQualificationCompletedEvent(
            Id,
            GetType().Name,
            QualificationTime.Value));
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientLoginRecordedEvent(Id, GetType().Name, LastLogin.Value));
    }

    public void RecordCommunication()
    {
        LastCommunication = DateTime.UtcNow;
        LastUpdate = DateTime.UtcNow;

        AddDomainEvent(new ClientCommunicationRecordedEvent(
            Id,
            GetType().Name,
            LastCommunication.Value));
    }
}