namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientKycStatusChangedEvent : DomainEvent
{
    public string? PreviousKycStatus { get; }
    public string? NewKycStatus { get; }

    public ClientKycStatusChangedEvent(
        Guid aggregateId,
        string aggregateType,
        string? previousKycStatus,
        string? newKycStatus) : base(aggregateId, aggregateType)
    {
        PreviousKycStatus = previousKycStatus;
        NewKycStatus = newKycStatus;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}