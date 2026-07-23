namespace TaskFlow.Api.Domain.Events;

public record TaskCreatedEvent(Guid TaskId, string Title, DateTime CreatedAt);

public record TaskCompletedEvent(Guid TaskId, string Title, DateTime CompletedAt);
