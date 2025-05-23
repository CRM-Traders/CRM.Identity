namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateCreatedEvent : DomainEvent
{ 
    public string? Phone { get; }
    public string? Website { get; }

    public AffiliateCreatedEvent(
        Guid aggregateId,
        string aggregateType, 
        string? phone,
        string? website) : base(aggregateId, aggregateType)
    { 
        Phone = phone;
        Website = website;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
