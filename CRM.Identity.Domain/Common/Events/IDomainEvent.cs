namespace CRM.Identity.Domain.Common.Events;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredOn { get; }
    Guid AggregateId { get; }
    string AggregateType { get; }
}