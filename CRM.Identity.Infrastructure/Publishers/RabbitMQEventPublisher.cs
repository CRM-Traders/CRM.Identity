using CRM.Identity.Domain.Common.Options.RabbitMq;
using CRM.Identity.Infrastructure.Messages;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CRM.Identity.Infrastructure.Publishers;

public class RabbitMQEventPublisher : IExternalEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    private readonly RabbitMQOptions _options;
    private readonly string _serviceName;

    public RabbitMQEventPublisher(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQEventPublisher> logger)
    {
        _logger = logger;
        _options = options.Value;
        _serviceName = "CRM.Identity";

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            Port = _options.Port
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ publisher connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ publisher connection");
            throw;
        }
    }

    public Task PublishEventAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventMessage = new EventMessage
            {
                Id = outboxMessage.Id,
                EventType = outboxMessage.Type,
                ServiceName = _serviceName,
                AggregateId = outboxMessage.AggregateId,
                AggregateType = outboxMessage.AggregateType,
                OccurredOn = outboxMessage.CreatedAt,
                Content = outboxMessage.Content,
                Metadata = new Dictionary<string, string>
                {
                    { "ProcessedAt", DateTimeOffset.UtcNow.ToString("o") }
                }
            };

            var messageJson = JsonSerializer.Serialize(eventMessage);
            var body = Encoding.UTF8.GetBytes(messageJson);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            var routingKey = $"events.{outboxMessage.AggregateType.ToLower()}";

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published event message {EventId} of type {EventType} with routing key {RoutingKey}",
                eventMessage.Id, eventMessage.EventType, routingKey);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event message {EventId} of type {EventType}",
                outboxMessage.Id, outboxMessage.Type);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ publisher connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ publisher");
        }
    }
}