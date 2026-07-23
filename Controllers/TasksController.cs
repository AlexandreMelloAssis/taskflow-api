using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Domain.Events;
using TaskFlow.Api.DTOs;
using TaskFlow.Api.Infrastructure.Messaging;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public TasksController(ITaskRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _repository.GetAllAsync();
        return Ok(tasks.Select(TaskDto.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        var task = await _repository.GetByIdAsync(id);
        return task is null ? NotFound() : Ok(TaskDto.FromEntity(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest("Title is required.");
        }

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description
        };

        var created = await _repository.CreateAsync(task);

        _eventPublisher.Publish(
            "task.created",
            new TaskCreatedEvent(created.Id, created.Title, created.CreatedAt));

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, TaskDto.FromEntity(created));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTaskStatusDto dto)
    {
        var updated = await _repository.UpdateStatusAsync(id, dto.Status);
        if (!updated)
        {
            return NotFound();
        }

        if (dto.Status == TaskItemStatus.Done)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task is not null)
            {
                _eventPublisher.Publish(
                    "task.completed",
                    new TaskCompletedEvent(task.Id, task.Title, task.CompletedAt ?? DateTime.UtcNow));
            }
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
