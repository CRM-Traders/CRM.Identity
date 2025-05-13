namespace CRM.Identity.Infrastructure.Publishers;

public class EventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(
        IServiceProvider serviceProvider,
        ILogger<EventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var domainEventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        if (!handlers.Any())
        {
            _logger.LogWarning("No handlers registered for {DomainEventType}", domainEventType.Name);
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                await (Task)handlerType
                    .GetMethod("HandleAsync")!
                    .Invoke(handler, new object[] { domainEvent, cancellationToken })!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {DomainEventType}", domainEventType.Name);
                throw;
            }
        }
    }
}