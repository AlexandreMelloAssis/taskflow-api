using TaskFlow.Api.Domain;

namespace TaskFlow.Api.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt)
{
    public static TaskDto FromEntity(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.CreatedAt,
        task.CompletedAt);
}

public record CreateTaskDto(string Title, string? Description);

public record UpdateTaskStatusDto(TaskItemStatus Status);
