using CRM.Identity.Application.Common.Services.Outbox;

namespace CRM.Identity.Infrastructure.Services.Outbox;

public class OutboxService(
    IOutboxRepository _outboxRepository,
    IEventPublisher _eventPublisher,
    ILogger<OutboxService> _logger) : IOutboxService
{
    public async Task<IReadOnlyDictionary<Guid, OutboxMessage>> CreateOutboxMessagesAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, OutboxMessage>();

        foreach (var domainEvent in domainEvents)
        {
            var serializedContent = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            var outboxMessage = OutboxMessage.Create(
                domainEvent,
                domainEvent.AggregateId,
                domainEvent.AggregateType,
                serializedContent);

            if (domainEvent.ProcessingStrategy == ProcessingStrategy.Immediate)
            {
                outboxMessage.MarkForImmediateProcessing();
            }

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
            result[domainEvent.Id] = outboxMessage;
        }

        return result;
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 20;
        var messages = await _outboxRepository.GetUnprocessedMessagesAsync(batchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var domainEventType = Type.GetType(message.Type);
                if (domainEventType == null)
                {
                    _logger.LogError("Cannot find type {EventType}", message.Type);
                    message.MarkAsFailed($"Cannot find type {message.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, domainEventType) as IDomainEvent;
                if (domainEvent == null)
                {
                    _logger.LogError("Cannot deserialize event {EventType}", message.Type);
                    message.MarkAsFailed($"Cannot deserialize event {message.Type}");
                    continue;
                }

                await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
                message.MarkAsProcessed();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                message.MarkAsFailed(ex.Message);
            }
        }
    }
}