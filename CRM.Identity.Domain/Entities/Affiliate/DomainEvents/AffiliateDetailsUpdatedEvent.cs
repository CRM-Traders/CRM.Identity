namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateDetailsUpdatedEvent : DomainEvent
{ 
    public string? Phone { get; }
    public string? Website { get; }

    public AffiliateDetailsUpdatedEvent(
        Guid aggregateId,
        string aggregateType, 
        string? phone,
        string? website) : base(aggregateId, aggregateType)
    { 
        Phone = phone;
        Website = website;

        ProcessingStrategy = ProcessingStrategy.Background;
    }
}