namespace CRM.Identity.Application.Common.Abstractions;

public interface IOutboxService
{
    Task SaveEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default);
}