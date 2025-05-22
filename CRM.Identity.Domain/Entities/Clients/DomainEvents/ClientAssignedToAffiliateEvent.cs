namespace CRM.Identity.Domain.Entities.Clients.DomainEvents;

public sealed class ClientAssignedToAffiliateEvent : DomainEvent
{
    public Guid PreviousAffiliateId { get; }
    public Guid NewAffiliateId { get; }

    public ClientAssignedToAffiliateEvent(
        Guid aggregateId,
        string aggregateType,
        Guid previousAffiliateId,
        Guid newAffiliateId) : base(aggregateId, aggregateType)
    {
        PreviousAffiliateId = previousAffiliateId;
        NewAffiliateId = newAffiliateId;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}