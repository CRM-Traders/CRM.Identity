public class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IExternalEventPublisher _externalEventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxRepository outboxRepository,
        IEventPublisher eventPublisher,
        IExternalEventPublisher externalEventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<OutboxProcessor> logger)
    {
        _outboxRepository = outboxRepository;
        _eventPublisher = eventPublisher;
        _externalEventPublisher = externalEventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 20;

        var messages = await _outboxRepository.GetUnprocessedMessagesAsync(batchSize, cancellationToken);
        _logger.LogInformation("Found {Count} messages to process", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var processed = await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    if (!await _outboxRepository.TryClaimMessageAsync(message.Id, "processor", cancellationToken))
                    {
                        return false;
                    }

                    try
                    {
                        var domainEventType = Type.GetType(message.Type);
                        if (domainEventType == null)
                        {
                            message.MarkAsFailed($"Cannot find type {message.Type}");
                            return false;
                        }

                        var domainEvent = JsonSerializer.Deserialize(message.Content, domainEventType) as IDomainEvent;
                        if (domainEvent == null)
                        {
                            message.MarkAsFailed($"Cannot deserialize event {message.Type}");
                            return false;
                        }

                        await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
                        await _externalEventPublisher.PublishEventAsync(message, cancellationToken);

                        message.MarkAsProcessed();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                        message.MarkAsFailed(ex.Message);
                        return false;
                    }
                }, cancellationToken);

                if (processed)
                {
                    _logger.LogInformation("Successfully processed message {MessageId}", message.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction error for message {MessageId}", message.Id);
            }
        }
    }
}