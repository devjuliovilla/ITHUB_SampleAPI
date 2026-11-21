using ITHUB.SampleAPI.Domain.Entities;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public interface ITaskRepository
{
    Task<bool> IsActiveUserAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskEntity>> GetTasksAsync(long? assignedUserId, CancellationToken cancellationToken);
    Task<TaskEntity?> GetTaskAsync(long taskId, CancellationToken cancellationToken);
    Task AddTaskAsync(TaskEntity task, CancellationToken cancellationToken);
    Task AddActivityAsync(TaskActivityEntity activity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
