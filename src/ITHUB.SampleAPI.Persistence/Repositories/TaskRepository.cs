using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public sealed class TaskRepository(SampleApiDbContext dbContext) : ITaskRepository
{
    public Task<bool> IsActiveUserAsync(long userId, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(user => user.Id == userId && user.IsActive, cancellationToken);

    public async Task<IReadOnlyList<TaskEntity>> GetTasksAsync(long? assignedUserId, CancellationToken cancellationToken)
    {
        var query = dbContext.Tasks.AsNoTracking()
            .Include(task => task.Status)
            .Include(task => task.Activities)
                .ThenInclude(activity => activity.Status)
            .AsQueryable();
        if (assignedUserId.HasValue)
            query = query.Where(task => task.AssignedUserId == assignedUserId.Value);
        return await query.OrderByDescending(task => task.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task<TaskEntity?> GetTaskAsync(long taskId, CancellationToken cancellationToken) =>
        dbContext.Tasks
            .Include(task => task.Status)
            .Include(task => task.Activities)
                .ThenInclude(activity => activity.Status)
            .SingleOrDefaultAsync(task => task.Id == taskId, cancellationToken);

    public async Task AddTaskAsync(TaskEntity task, CancellationToken cancellationToken)
    {
        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddActivityAsync(TaskActivityEntity activity, CancellationToken cancellationToken)
    {
        dbContext.TaskActivities.Add(activity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
