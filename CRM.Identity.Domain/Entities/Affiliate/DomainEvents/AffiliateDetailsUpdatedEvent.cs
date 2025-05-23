namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateDetailsUpdatedEvent : DomainEvent
{
    public string Name { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string? Website { get; }

    public AffiliateDetailsUpdatedEvent(
        Guid aggregateId,
        string aggregateType,
        string name,
        string email,
        string? phone,
        string? website) : base(aggregateId, aggregateType)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Website = website;

        ProcessingStrategy = ProcessingStrategy.Background;
    }
}