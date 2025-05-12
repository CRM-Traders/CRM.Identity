namespace CRM.Identity.Application.Common.Services;

public interface IOutboxService
{
    Task SaveEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default);
}