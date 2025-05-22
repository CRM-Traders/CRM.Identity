using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientStatusChangedEvent : DomainEvent
{
    public ClientStatus PreviousStatus { get; }
    public ClientStatus NewStatus { get; }

    public ClientStatusChangedEvent(
        Guid aggregateId,
        string aggregateType,
        ClientStatus previousStatus,
        ClientStatus newStatus) : base(aggregateId, aggregateType)
    {
        PreviousStatus = previousStatus;
        NewStatus = newStatus;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}