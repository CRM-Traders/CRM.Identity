namespace CRM.Identity.Persistence.Repositories;

public class OutboxRepository(ApplicationDbContext _dbContext) : Repository<OutboxMessage>(_dbContext), IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        int maxMessages,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && !m.IsClaimed)
            .OrderByDescending(m => m.Priority)
            .ThenBy(m => m.CreatedAt)
            .Take(maxMessages)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesForPartitionAsync(
        int partitionId,
        int partitionCount,
        int maxMessages,
        CancellationToken cancellationToken = default)
    {
        var messageIds = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && !m.IsClaimed)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var partitionedIds = messageIds
            .Where(id => Math.Abs(id.GetHashCode()) % partitionCount + 1 == partitionId)
            .Take(maxMessages);

        return await _dbContext.OutboxMessages
            .Where(m => partitionedIds.Contains(m.Id))
            .OrderByDescending(m => m.Priority)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryClaimMessageAsync(
        Guid messageId,
        string instanceId,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var message = await _dbContext.OutboxMessages
                .FromSqlRaw("SELECT * FROM \"OutboxMessage\" WHERE \"Id\" = {0} FOR UPDATE", messageId)
                .FirstOrDefaultAsync(cancellationToken);

            if (message != null && !message.IsClaimed)
            {
                message.ClaimForProcessing(instanceId);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            }

            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}