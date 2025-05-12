using CRM.Identity.Application.Common.Abstractions.Mediators;

namespace CRM.Identity.Application.Common.Persistence.Repositories;

public interface IOutboxRepository : IRepository<OutboxMessage>
{
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int maxMessages, CancellationToken cancellationToken = default);
}