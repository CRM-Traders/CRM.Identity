namespace CRM.Identity.Infrastructure.Workers;

public class OutboxProcessorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorWorker> logger,
        IConnectionMultiplexer redis)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                await outboxProcessor.ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processing cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
