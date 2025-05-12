namespace CRM.Identity.Domain.Entities.OutboxMessages;

public class OutboxMessage : Entity
{
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    private OutboxMessage(
        Guid id,
        string type,
        string content,
        DateTimeOffset createdAt) : base(id)
    {
        Type = type;
        Content = content;
        CreatedAt = createdAt;
        RetryCount = 0;
    }

    public static OutboxMessage Create(IDomainEvent domainEvent, string serializedContent)
    {
        return new OutboxMessage(
            domainEvent.Id,
            domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
            serializedContent,
            domainEvent.OccurredOn);
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
    }

    public void ClearError()
    {
        Error = null;
    }
}