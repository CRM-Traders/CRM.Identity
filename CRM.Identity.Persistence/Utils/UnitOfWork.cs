namespace CRM.Identity.Persistence.Utils;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOutboxService _outboxService;
    private readonly IEventPublisher _eventPublisher;

    public UnitOfWork(
        ApplicationDbContext dbContext,
        IOutboxService outboxService,
        IEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _outboxService = outboxService;
        _eventPublisher = eventPublisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var domainEvents = GetDomainEventsFromTrackedEntities();
            ClearDomainEvents();

            var outboxMessages = await _outboxService.CreateOutboxMessagesAsync(domainEvents, cancellationToken);

            var result = await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await ProcessImmediateEventsAsync(domainEvents, outboxMessages, cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ProcessImmediateEventsAsync(
        IEnumerable<IDomainEvent> immediateEvents,
        IReadOnlyDictionary<Guid, OutboxMessage> outboxMessages,
        CancellationToken cancellationToken)
    {
        foreach (var @event in immediateEvents)
        {
            try
            {
                await _eventPublisher.PublishAsync(@event, cancellationToken);

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (outboxMessages.TryGetValue(@event.Id, out var outboxMessage))
                {
                    outboxMessage.MarkAsProcessed();
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                scope.Complete();
            }
            catch (Exception ex)
            {
            }
        }
    }

    private IReadOnlyList<IDomainEvent> GetDomainEventsFromTrackedEntities()
    {
        var domainEvents = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        return domainEvents;
    }

    private void ClearDomainEvents()
    {
        _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .ToList()
            .ForEach(e => e.Entity.ClearDomainEvents());
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation();
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}