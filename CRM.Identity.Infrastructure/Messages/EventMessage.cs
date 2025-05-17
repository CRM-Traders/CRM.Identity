namespace CRM.Identity.Infrastructure.Messages;

public class EventMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public DateTimeOffset OccurredOn { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}