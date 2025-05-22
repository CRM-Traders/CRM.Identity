namespace CRM.Identity.Domain.Entities.Leads.DomainEvents;

public sealed class LeadConvertedToClientEvent : DomainEvent
{
    public Guid ClientId { get; }
    public Guid AffiliateId { get; }

    public LeadConvertedToClientEvent(
        Guid aggregateId,
        string aggregateType,
        Guid clientId,
        Guid affiliateId) : base(aggregateId, aggregateType)
    {
        ClientId = clientId;
        AffiliateId = affiliateId;

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}