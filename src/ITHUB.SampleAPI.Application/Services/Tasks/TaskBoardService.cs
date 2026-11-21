using ITHUB.SampleAPI.Application.Contracts.Tasks;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Repositories;
using Mapster;

namespace ITHUB.SampleAPI.Application.Services.Tasks;

public sealed class TaskBoardService(
    ITaskRepository taskRepository,
    IStatusRepository statusRepository,
    TypeAdapterConfig mappingConfiguration) : ITaskBoardService
{
    public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(bool isAdmin, long userId, CancellationToken cancellationToken)
    {
        var tasks = await taskRepository.GetTasksAsync(isAdmin ? null : userId, cancellationToken);
        return tasks.Select(task => task.Adapt<TaskDto>(mappingConfiguration)).ToArray();
    }

    public async Task<IReadOnlyList<StatusDto>> GetStatusesAsync(string scope, CancellationToken cancellationToken)
    {
        var statuses = await statusRepository.GetActiveAsync(scope, cancellationToken);
        return statuses.Adapt<IReadOnlyList<StatusDto>>(mappingConfiguration);
    }

    public async Task<TaskDto?> GetTaskAsync(long taskId, bool isAdmin, long userId, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetTaskAsync(taskId, cancellationToken);
        return task is null || (!isAdmin && task.AssignedUserId != userId)
            ? null
            : task.Adapt<TaskDto>(mappingConfiguration);
    }

    public async Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request, long adminUserId, CancellationToken cancellationToken)
    {
        if (!await taskRepository.IsActiveUserAsync(request.AssignedUserId, cancellationToken))
            return null;

        var taskStatus = await statusRepository.GetByCodeAsync(
            StatusCatalogConstants.TaskAssigned,
            StatusCatalogConstants.TaskScope,
            cancellationToken)
            ?? throw new InvalidOperationException("The Assigned task status is missing from the status catalog.");

        var now = DateTime.UtcNow;
        var task = new TaskEntity
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            AssignedUserId = request.AssignedUserId,
            CreatedByUserId = adminUserId,
            StatusId = taskStatus.Id,
            Status = taskStatus,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        await taskRepository.AddTaskAsync(task, cancellationToken);
        return task.Adapt<TaskDto>(mappingConfiguration);
    }

    public async Task<TaskActivityDto?> AddActivityAsync(
        long taskId,
        CreateActivityRequest request,
        bool isAdmin,
        long userId,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetTaskAsync(taskId, cancellationToken);
        if (task is null || (!isAdmin && task.AssignedUserId != userId))
            return null;

        var pendingStatus = await statusRepository.GetByCodeAsync(
            StatusCatalogConstants.ActivityPending,
            StatusCatalogConstants.ActivityScope,
            cancellationToken)
            ?? throw new InvalidOperationException("The Pending activity status is missing from the status catalog.");

        var now = DateTime.UtcNow;
        var activity = new TaskActivityEntity
        {
            TaskId = taskId,
            CreatedByUserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            StatusId = pendingStatus.Id,
            Status = pendingStatus,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        await taskRepository.AddActivityAsync(activity, cancellationToken);
        return activity.Adapt<TaskActivityDto>(mappingConfiguration);
    }

    public async Task<bool> UpdateActivityStatusAsync(
        long taskId,
        long activityId,
        int statusId,
        bool isAdmin,
        long userId,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetTaskAsync(taskId, cancellationToken);
        if (task is null || (!isAdmin && task.AssignedUserId != userId))
            return false;

        var activity = task.Activities.SingleOrDefault(item => item.Id == activityId);
        if (activity is null || (!isAdmin && activity.CreatedByUserId != userId))
            return false;

        var status = await statusRepository.GetByIdAsync(
            statusId,
            StatusCatalogConstants.ActivityScope,
            cancellationToken);
        if (status is null)
            return false;

        activity.StatusId = status.Id;
        activity.Status = status;
        activity.UpdatedAtUtc = DateTime.UtcNow;
        await taskRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

}
