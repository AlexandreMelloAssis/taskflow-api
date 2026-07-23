using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Domain;
using TaskFlow.Api.Infrastructure;
using Xunit;

namespace TaskFlow.Api.Tests;

public class TaskRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_AddsTaskToDatabase()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);
        var task = new TaskItem { Title = "Write unit tests" };

        var created = await repository.CreateAsync(task);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Write unit tests", created.Title);
        Assert.Single(await repository.GetAllAsync());
    }

    [Fact]
    public async Task UpdateStatusAsync_SetsCompletedAt_WhenStatusIsDone()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);
        var task = await repository.CreateAsync(new TaskItem { Title = "Ship feature" });

        var updated = await repository.UpdateStatusAsync(task.Id, TaskItemStatus.Done);
        var stored = await repository.GetByIdAsync(task.Id);

        Assert.True(updated);
        Assert.Equal(TaskItemStatus.Done, stored!.Status);
        Assert.NotNull(stored.CompletedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTask_WhenItExists()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);
        var task = await repository.CreateAsync(new TaskItem { Title = "Temporary task" });

        var deleted = await repository.DeleteAsync(task.Id);

        Assert.True(deleted);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        await using var context = CreateContext();
        var repository = new TaskRepository(context);

        var deleted = await repository.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }
}
