using ITHUB.SampleAPI.Application.Contracts.Tasks;

namespace ITHUB.SampleAPI.Application.Services.Tasks;

public interface ITaskBoardService
{
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(bool isAdmin, long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StatusDto>> GetStatusesAsync(string scope, CancellationToken cancellationToken);
    Task<TaskDto?> GetTaskAsync(long taskId, bool isAdmin, long userId, CancellationToken cancellationToken);
    Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request, long adminUserId, CancellationToken cancellationToken);
    Task<TaskActivityDto?> AddActivityAsync(long taskId, CreateActivityRequest request, bool isAdmin, long userId, CancellationToken cancellationToken);
    Task<bool> UpdateActivityStatusAsync(long taskId, long activityId, int statusId, bool isAdmin, long userId, CancellationToken cancellationToken);
}
