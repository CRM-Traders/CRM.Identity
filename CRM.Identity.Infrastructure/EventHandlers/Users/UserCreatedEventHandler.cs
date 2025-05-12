namespace CRM.Identity.Infrastructure.EventHandlers.Users;

public class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User created with ID: {UserId}", domainEvent.AggregateId);

        // Here you could add additional logic like:
        // - Send welcome email
        // - Create default settings for the user
        // - Notify administrators

        await Task.CompletedTask;
    }
}