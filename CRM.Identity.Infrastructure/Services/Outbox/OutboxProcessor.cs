namespace CRM.Identity.Infrastructure.Services.Outbox;

public class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxService _outboxService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxService outboxService,
        IUnitOfWork unitOfWork,
        ILogger<OutboxProcessor> logger)
    {
        _outboxService = outboxService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _outboxService.ProcessOutboxMessagesAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing outbox messages");
        }
    }
}