namespace TaskFlow.Api.Infrastructure.Messaging;

public interface IEventPublisher
{
    void Publish<T>(string routingKey, T @event);
}
