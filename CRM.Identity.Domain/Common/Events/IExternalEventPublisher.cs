using CRM.Identity.Domain.Entities.OutboxMessages;

namespace CRM.Identity.Domain.Common.Events;

public interface IExternalEventPublisher
{
    Task PublishEventAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default);
}