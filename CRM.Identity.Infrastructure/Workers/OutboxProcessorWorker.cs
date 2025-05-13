namespace CRM.Identity.Infrastructure.Workers;

public class OutboxProcessorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorWorker> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly string _instanceId;

    public OutboxProcessorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorWorker> logger,
        IConnectionMultiplexer redis)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _redis = redis;
        _instanceId = Guid.NewGuid().ToString("N");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor starting with instance ID: {InstanceId}", _instanceId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var db = _redis.GetDatabase();
                var lockKey = "outbox:processing:lock";
                var lockValue = _instanceId;
                var lockExpiry = TimeSpan.FromMinutes(5);

                var lockAcquired = await db.LockTakeAsync(lockKey, lockValue, lockExpiry);

                if (lockAcquired)
                {
                    try
                    {
                        _logger.LogInformation("Lock acquired by instance {InstanceId}. Processing outbox messages.", _instanceId);

                        using var scope = _scopeFactory.CreateScope();
                        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                        await outboxProcessor.ProcessPendingMessagesAsync(stoppingToken);
                    }
                    finally
                    {
                        await db.LockReleaseAsync(lockKey, lockValue);
                        _logger.LogInformation("Lock released by instance {InstanceId}.", _instanceId);
                    }
                }
                else
                {
                    _logger.LogDebug("Lock already held by another instance. Skipping this cycle.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processing cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
