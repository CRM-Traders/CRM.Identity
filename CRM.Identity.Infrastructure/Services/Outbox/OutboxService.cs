namespace CRM.Identity.Infrastructure.Services.Outbox;

public class OutboxService : IOutboxService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OutboxService> _logger;
    private readonly string _instanceId;

    public OutboxService(
        IOutboxRepository outboxRepository,
        IEventPublisher eventPublisher,
        ILogger<OutboxService> logger)
    {
        _outboxRepository = outboxRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _instanceId = Guid.NewGuid().ToString("N");
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

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 20;
        var messages = await _outboxRepository.GetUnprocessedMessagesAsync(batchSize, cancellationToken);

        foreach (var message in messages)
        {
            if (await TryClaimMessageAsync(message.Id, cancellationToken))
            {
                try
                {
                    await ProcessMessageAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    message.MarkAsFailed(ex.Message);
                }
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

        foreach (var message in messages)
        {
            if (await TryClaimMessageAsync(message.Id, cancellationToken))
            {
                try
                {
                    await ProcessMessageAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    message.MarkAsFailed(ex.Message);
                }
            }
        }
    }

    private async Task<bool> TryClaimMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return await _outboxRepository.TryClaimMessageAsync(messageId, _instanceId, cancellationToken);
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
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
        message.MarkAsProcessed();
    }
}