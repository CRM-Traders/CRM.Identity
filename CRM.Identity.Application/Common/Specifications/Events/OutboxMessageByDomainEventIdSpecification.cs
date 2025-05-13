namespace CRM.Identity.Application.Common.Specifications.Events;

public class OutboxMessageByDomainEventIdSpecification : BaseSpecification<OutboxMessage>
{
    public OutboxMessageByDomainEventIdSpecification(Guid domainEventId)
        : base(message => message.Id == domainEventId)
    {
    }
}