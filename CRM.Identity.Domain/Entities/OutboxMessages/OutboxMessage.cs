namespace CRM.Identity.Domain.Entities.OutboxMessages;

public class OutboxMessage : Entity
{
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; }
    public bool IsClaimed { get; private set; }
    public string? ClaimedBy { get; private set; }
    public DateTimeOffset? ClaimedAt { get; private set; }
    public MessagePriority Priority { get; private set; } = MessagePriority.Normal;

    private OutboxMessage(
        Guid id,
        Guid aggregateId,
        string aggregateType,
        string type,
        string content,
        DateTimeOffset createdAt) : base(id)
    {
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        Type = type;
        Content = content;
        CreatedAt = createdAt;
        RetryCount = 0;
    }

    public static OutboxMessage Create(IDomainEvent domainEvent, Guid aggregateId, string aggregateType, string serializedContent)
    {
        return new OutboxMessage(
            domainEvent.Id,
            aggregateId,
            aggregateType,
            domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
            serializedContent,
            domainEvent.OccurredOn);
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void MarkForImmediateProcessing()
    {
        Priority = MessagePriority.High;
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

    public bool ClaimForProcessing(string instanceId)
    {
        if (IsClaimed)
            return false;

        IsClaimed = true;
        ClaimedBy = instanceId;
        ClaimedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public void ReleaseClaim()
    {
        IsClaimed = false;
        ClaimedBy = null;
        ClaimedAt = null;
    }
}

public enum MessagePriority
{
    Normal = 0,
    High = 1
}