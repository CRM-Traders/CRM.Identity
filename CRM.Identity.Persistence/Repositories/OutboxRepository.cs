namespace CRM.Identity.Persistence.Repositories;

public class OutboxRepository(ApplicationDbContext _dbContext) : Repository<OutboxMessage>(_dbContext), IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        int maxMessages,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(m => !m.IsClaimed)
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
            .Where(m => !m.IsClaimed)
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
        try
        {
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(
                @"UPDATE ""OutboxMessage"" 
               SET ""IsClaimed"" = true, 
                   ""ClaimedBy"" = {0}, 
                   ""ClaimedAt"" = {1} 
               WHERE ""Id"" = {2} 
                 AND ""IsClaimed"" = false 
                 AND ""ProcessedAt"" IS NULL",
                new object[] { instanceId, DateTimeOffset.UtcNow, messageId },
                cancellationToken);

            return rowsAffected > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}