namespace CRM.Identity.Infrastructure.Services.Outbox;

public class OutboxProcessor : IOutboxProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly string _instanceId;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        IConnectionMultiplexer redis)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _redis = redis;
        _instanceId = Guid.NewGuid().ToString("N");
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var partitionKey = $"outbox:instance:{_instanceId}:partition";
        var partitionCount = 16; 

        var partitionId = (int)await db.StringGetAsync(partitionKey);
        if (partitionId == 0)
        {
            partitionId = Random.Shared.Next(1, partitionCount + 1);
            await db.StringSetAsync(partitionKey, partitionId, TimeSpan.FromHours(1));
        }

        _logger.LogInformation("Processing outbox messages for partition {PartitionId}", partitionId);

        using var scope = _serviceProvider.CreateScope();
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await outboxService.ProcessOutboxMessagesForPartitionAsync(partitionId, partitionCount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}