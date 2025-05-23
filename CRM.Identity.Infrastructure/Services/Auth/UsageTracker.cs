using System.Threading.Channels;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Infrastructure.Services.Auth;

public sealed class UsageTracker : IUsageTracker, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UsageTracker> _logger;
    private readonly Channel<Guid> _channel;
    private readonly Task _processor;
    private readonly CancellationTokenSource _cts = new();

    public UsageTracker(IServiceScopeFactory scopeFactory, ILogger<UsageTracker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
 
        var options = new BoundedChannelOptions(5000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        };

        _channel = Channel.CreateBounded<Guid>(options);
        _processor = ProcessAsync(_cts.Token);
    }

    public ValueTask TrackAsync(Guid secretId, string? clientIp = null)
    { 
        return _channel.Writer.TryWrite(secretId)
            ? ValueTask.CompletedTask
            : new ValueTask(Task.CompletedTask);
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var batch = new List<Guid>(100);

        await foreach (var secretId in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            batch.Add(secretId);

            if (batch.Count >= 50)
            {
                await UpdateBatchAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            await UpdateBatchAsync(batch);
    }

    private async Task UpdateBatchAsync(List<Guid> secretIds)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<AffiliateSecret>>();
 
            var groups = secretIds.GroupBy(x => x).ToList();

            foreach (var group in groups)
            {
                var secret = await repository.GetByIdAsync(group.Key);
                if (secret?.IsActive == true && !secret.IsExpired())
                { 
                    foreach (var _ in group)
                        secret.TryUse();

                    await repository.UpdateAsync(secret);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch update failed");
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        _processor.Wait(2000);
        _cts.Dispose();
    }
}