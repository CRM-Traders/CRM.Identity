namespace CRM.Identity.Domain.Entities.Affiliate.DomainEvents;

public sealed class AffiliateCreatedEvent : DomainEvent
{
    public string Name { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string? Website { get; }

    public AffiliateCreatedEvent(
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

        ProcessingStrategy = ProcessingStrategy.Immediate;
    }
}
