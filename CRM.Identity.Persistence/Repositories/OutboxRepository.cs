namespace CRM.Identity.Persistence.Repositories;

public class OutboxRepository(ApplicationDbContext _dbContext) : Repository<OutboxMessage>(_dbContext), IOutboxRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int maxMessages, CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(maxMessages)
            .ToListAsync(cancellationToken);
    }
}