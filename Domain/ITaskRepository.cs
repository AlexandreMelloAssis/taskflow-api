namespace TaskFlow.Api.Domain;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<bool> UpdateStatusAsync(Guid id, TaskItemStatus status);
    Task<bool> DeleteAsync(Guid id);
}
