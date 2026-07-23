using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace TaskFlow.Api.Infrastructure.Messaging;

/// <summary>
/// Publishes domain events to RabbitMQ. Connects lazily on first publish and treats
/// broker unavailability as non-fatal: messaging is a side-channel, so a down broker
/// should never break the core request (create/update/delete still succeed).
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private const string ExchangeName = "taskflow.events";

    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly object _lock = new();
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Publish<T>(string routingKey, T @event)
    {
        try
        {
            EnsureChannel();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            var properties = _channel!.CreateBasicProperties();
            properties.ContentType = "application/json";
            properties.Persistent = true;

            _channel.BasicPublish(ExchangeName, routingKey, properties, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to publish event with routing key {RoutingKey} to RabbitMQ. Continuing without messaging.",
                routingKey);
        }
    }

    private void EnsureChannel()
    {
        if (_channel is { IsOpen: true })
        {
            return;
        }

        lock (_lock)
        {
            if (_channel is { IsOpen: true })
            {
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:HostName"] ?? "localhost",
                Port = int.TryParse(_configuration["RabbitMq:Port"], out var port) ? port : 5672,
                UserName = _configuration["RabbitMq:UserName"] ?? "guest",
                Password = _configuration["RabbitMq:Password"] ?? "guest",
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
