namespace CRM.Identity.Infrastructure.Services;

public class OutboxService(
    IOutboxRepository _outboxRepository,
    IEventPublisher _eventPublisher,
    ILogger<OutboxService> _logger,
    IUnitOfWork _unitOfWork) : IOutboxService
{
    public async Task SaveEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var serializedContent = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            var outboxMessage = OutboxMessage.Create(
                domainEvent,
                domainEvent.AggregateId,
                domainEvent.AggregateType,
                serializedContent);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}