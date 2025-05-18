namespace CRM.Identity.Infrastructure.Services.Outbox;

public class OutboxService : IOutboxService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IExternalEventPublisher _externalEventPublisher;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(
        IOutboxRepository outboxRepository,
        IEventPublisher eventPublisher,
        IExternalEventPublisher externalEventPublisher,
        ILogger<OutboxService> logger)
    {
        _outboxRepository = outboxRepository;
        _eventPublisher = eventPublisher;
        _externalEventPublisher = externalEventPublisher;
        _logger = logger;
    }

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

    public async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var domainEventType = Type.GetType(message.Type);
            if (domainEventType == null)
            {
                _logger.LogError("Cannot find type {EventType}", message.Type);
                message.MarkAsFailed($"Cannot find type {message.Type}");
                return;
            }

            var domainEvent = JsonSerializer.Deserialize(message.Content, domainEventType) as IDomainEvent;
            if (domainEvent == null)
            {
                _logger.LogError("Cannot deserialize event {EventType}", message.Type);
                message.MarkAsFailed($"Cannot deserialize event {message.Type}");
                return;
            }

            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
            await _externalEventPublisher.PublishEventAsync(message, cancellationToken);

            message.MarkAsProcessed();
            _logger.LogInformation("Successfully processed message {MessageId} of type {EventType}",
                message.Id, message.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
            message.MarkAsFailed(ex.Message);
            throw;
        }
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        int maxMessages,
        CancellationToken cancellationToken = default)
    {
        return await _outboxRepository.GetUnprocessedMessagesAsync(maxMessages, cancellationToken);
    }

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 20;
        var messages = await _outboxRepository.GetUnprocessedMessagesAsync(batchSize, cancellationToken);

        _logger.LogInformation("Found {Count} unprocessed messages", messages.Count);

        foreach (var message in messages)
        {
            _logger.LogInformation("Processing message {MessageId}", message.Id);

            if (message.IsClaimed)
            {
                _logger.LogInformation("Message {MessageId} is already claimed by {ClaimedBy}",
                    message.Id, message.ClaimedBy);
                continue;
            }

            try
            {
                await ProcessMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
            }
        }
    }

    public async Task ProcessOutboxMessagesForPartitionAsync(
        int partitionId,
        int partitionCount,
        CancellationToken cancellationToken = default)
    {
        const int batchSize = 20;
        var messages = await _outboxRepository.GetUnprocessedMessagesForPartitionAsync(
            partitionId, partitionCount, batchSize, cancellationToken);

        _logger.LogInformation("Found {Count} messages for partition {PartitionId}/{PartitionCount}",
            messages.Count, partitionId, partitionCount);

        foreach (var message in messages)
        {
            _logger.LogInformation("Processing message {MessageId} in partition {PartitionId}",
                message.Id, partitionId);

            if (message.IsClaimed)
            {
                _logger.LogInformation("Message {MessageId} is already claimed by {ClaimedBy}",
                    message.Id, message.ClaimedBy);
                continue;
            }

            try
            {
                await ProcessMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId} in partition {PartitionId}",
                    message.Id, partitionId);
            }
        }
    }
}