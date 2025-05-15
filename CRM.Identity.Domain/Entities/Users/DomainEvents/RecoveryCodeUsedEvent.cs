namespace CRM.Identity.Domain.Entities.Users.DomainEvents;

public sealed class RecoveryCodeUsedEvent : DomainEvent
{
    public string Code { get; }

    public RecoveryCodeUsedEvent(Guid aggregateId, string aggregateType, string code)
        : base(aggregateId, aggregateType)
    {
        Code = code;
        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}