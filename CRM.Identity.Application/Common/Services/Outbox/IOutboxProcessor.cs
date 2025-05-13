namespace CRM.Identity.Application.Common.Services.Outbox;

public interface IOutboxProcessor
{
    Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
}
