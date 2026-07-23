using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Controllers;
using TaskFlow.Api.Domain;
using TaskFlow.Api.DTOs;
using Xunit;

namespace TaskFlow.Api.Tests;

public class TasksControllerTests
{
    private sealed class FakeTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = new();

        public Task<IEnumerable<TaskItem>> GetAllAsync() =>
            Task.FromResult<IEnumerable<TaskItem>>(_tasks);

        public Task<TaskItem?> GetByIdAsync(Guid id) =>
            Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

        public Task<TaskItem> CreateAsync(TaskItem task)
        {
            _tasks.Add(task);
            return Task.FromResult(task);
        }

        public Task<bool> UpdateStatusAsync(Guid id, TaskItemStatus status)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null)
            {
                return Task.FromResult(false);
            }

            task.Status = status;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null)
            {
                return Task.FromResult(false);
            }

            _tasks.Remove(task);
            return Task.FromResult(true);
        }
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenTitleIsEmpty()
    {
        var controller = new TasksController(new FakeTaskRepository());

        var result = await controller.Create(new CreateTaskDto("", null));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenTitleIsValid()
    {
        var controller = new TasksController(new FakeTaskRepository());

        var result = await controller.Create(new CreateTaskDto("Plan sprint", null));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<TaskDto>(created.Value);
        Assert.Equal("Plan sprint", dto.Title);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTaskIsMissing()
    {
        var controller = new TasksController(new FakeTaskRepository());

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
