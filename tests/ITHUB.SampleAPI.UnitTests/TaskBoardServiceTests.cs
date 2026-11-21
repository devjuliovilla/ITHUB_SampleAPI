using ITHUB.SampleAPI.Application.Contracts.Tasks;
using ITHUB.SampleAPI.Application.Mapping;
using ITHUB.SampleAPI.Application.Services.Tasks;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Repositories;
using Xunit;

namespace ITHUB.SampleAPI.UnitTests;

public sealed class TaskBoardServiceTests
{
    [Fact]
    public async Task AddActivityAsync_UserCannotAddToTaskAssignedToAnotherUser()
    {
        var repository = CreateRepository();
        var service = CreateService(repository);

        var activity = await service.AddActivityAsync(
            5,
            new CreateActivityRequest { Title = "My activity" },
            isAdmin: false,
            userId: 30,
            CancellationToken.None);

        Assert.Null(activity);
        Assert.Empty(repository.Task.Activities);
    }

    [Fact]
    public async Task UpdateActivityStatusAsync_UserCannotChangeAnotherUsersActivity()
    {
        var repository = CreateRepository();
        repository.Task.Activities.Add(new TaskActivityEntity
        {
            Id = 9,
            TaskId = 5,
            CreatedByUserId = 30,
            Title = "Other user's activity",
            StatusId = StatusCatalogConstants.ActivityPendingId,
            Status = CreateStatus(StatusCatalogConstants.ActivityPendingId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityPending)
        });
        var service = CreateService(repository);

        var updated = await service.UpdateActivityStatusAsync(
            5, 9, StatusCatalogConstants.ActivityCompletedId, isAdmin: false, userId: 20, CancellationToken.None);

        Assert.False(updated);
        Assert.Equal(StatusCatalogConstants.ActivityPendingId, repository.Task.Activities.Single().StatusId);
    }

    [Fact]
    public async Task UpdateActivityStatusAsync_AssignedUserCanChangeOwnActivityToKnownStatus()
    {
        var repository = CreateRepository();
        repository.Task.Activities.Add(new TaskActivityEntity
        {
            Id = 9,
            TaskId = 5,
            CreatedByUserId = 20,
            Title = "My activity",
            StatusId = StatusCatalogConstants.ActivityPendingId,
            Status = CreateStatus(StatusCatalogConstants.ActivityPendingId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityPending)
        });
        var service = CreateService(repository);

        var updated = await service.UpdateActivityStatusAsync(
            5, 9, StatusCatalogConstants.ActivityCompletedId, isAdmin: false, userId: 20, CancellationToken.None);

        Assert.True(updated);
        Assert.Equal(StatusCatalogConstants.ActivityCompletedId, repository.Task.Activities.Single().StatusId);
    }

    [Fact]
    public async Task GetTasksAsync_MapsCatalogStatusesToDtos()
    {
        var repository = CreateRepository();
        repository.Task.Activities.Add(new TaskActivityEntity
        {
            Id = 9,
            TaskId = 5,
            CreatedByUserId = 20,
            Title = "My activity",
            StatusId = StatusCatalogConstants.ActivityPendingId,
            Status = CreateStatus(StatusCatalogConstants.ActivityPendingId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityPending)
        });
        var service = CreateService(repository);

        var tasks = await service.GetTasksAsync(isAdmin: false, userId: 20, CancellationToken.None);

        Assert.Equal(StatusCatalogConstants.TaskAssignedId, tasks.Single().Status.Id);
        Assert.Equal(StatusCatalogConstants.TaskAssigned, tasks.Single().Status.Code);
        Assert.Equal(StatusCatalogConstants.ActivityPendingId, tasks.Single().Activities.Single().Status.Id);
        Assert.Equal(StatusCatalogConstants.ActivityPending, tasks.Single().Activities.Single().Status.Code);
    }

    [Fact]
    public async Task GetStatusesAsync_MapsCatalogRowsToDtos()
    {
        var repository = CreateRepository();
        var service = CreateService(repository);

        var statuses = await service.GetStatusesAsync(StatusCatalogConstants.ActivityScope, CancellationToken.None);

        Assert.Equal(2, statuses.Count);
        Assert.Contains(statuses, status => status.Id == StatusCatalogConstants.ActivityPendingId);
        Assert.Contains(statuses, status => status.Code == StatusCatalogConstants.ActivityCompleted);
        Assert.All(statuses, status => Assert.Equal(StatusCatalogConstants.ActivityScope, status.Scope));
    }

    private static InMemoryTaskRepository CreateRepository() => new()
    {
        Task = new TaskEntity
        {
            Id = 5,
            AssignedUserId = 20,
            CreatedByUserId = 10,
            StatusId = StatusCatalogConstants.TaskAssignedId,
            Status = CreateStatus(StatusCatalogConstants.TaskAssignedId, StatusCatalogConstants.TaskScope, StatusCatalogConstants.TaskAssigned)
        }
    };

    private static TaskBoardService CreateService(InMemoryTaskRepository repository) =>
        new(repository, new InMemoryStatusRepository(), SampleApiMappingConfiguration.Create());

    private static StatusEntity CreateStatus(int id, string scope, string code) => new()
    {
        Id = id,
        Scope = scope,
        Code = code,
        Name = code,
        IsActive = true
    };

    private sealed class InMemoryTaskRepository : ITaskRepository
    {
        public TaskEntity Task { get; set; } = new();

        public Task<bool> IsActiveUserAsync(long userId, CancellationToken cancellationToken) =>
            System.Threading.Tasks.Task.FromResult(userId == 20 || userId == 10);

        public Task<IReadOnlyList<TaskEntity>> GetTasksAsync(long? assignedUserId, CancellationToken cancellationToken)
        {
            IReadOnlyList<TaskEntity> result = assignedUserId is null || Task.AssignedUserId == assignedUserId
                ? [Task]
                : [];
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<TaskEntity?> GetTaskAsync(long taskId, CancellationToken cancellationToken) =>
            System.Threading.Tasks.Task.FromResult<TaskEntity?>(taskId == Task.Id ? Task : null);

        public Task AddTaskAsync(TaskEntity task, CancellationToken cancellationToken)
        {
            Task = task;
            task.Id = 5;
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public Task AddActivityAsync(TaskActivityEntity activity, CancellationToken cancellationToken)
        {
            activity.Id = 9;
            Task.Activities.Add(activity);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
    }

    private sealed class InMemoryStatusRepository : IStatusRepository
    {
        private static readonly StatusEntity[] Statuses =
        [
            CreateStatus(StatusCatalogConstants.TaskAssignedId, StatusCatalogConstants.TaskScope, StatusCatalogConstants.TaskAssigned),
            CreateStatus(StatusCatalogConstants.ActivityPendingId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityPending),
            CreateStatus(StatusCatalogConstants.ActivityCompletedId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityCompleted)
        ];

        public Task<StatusEntity?> GetByIdAsync(int statusId, string scope, CancellationToken cancellationToken) =>
            System.Threading.Tasks.Task.FromResult(Statuses.SingleOrDefault(status => status.Id == statusId && status.Scope == scope));

        public Task<StatusEntity?> GetByCodeAsync(string code, string scope, CancellationToken cancellationToken) =>
            System.Threading.Tasks.Task.FromResult(Statuses.SingleOrDefault(status => status.Code == code && status.Scope == scope));

        public Task<IReadOnlyList<StatusEntity>> GetActiveAsync(string scope, CancellationToken cancellationToken)
        {
            IReadOnlyList<StatusEntity> result = Statuses.Where(status => status.Scope == scope).ToArray();
            return System.Threading.Tasks.Task.FromResult(result);
        }
    }
}
