using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public sealed class StatusRepository(SampleApiDbContext dbContext) : IStatusRepository
{
    public Task<StatusEntity?> GetByIdAsync(int statusId, string scope, CancellationToken cancellationToken) =>
        dbContext.Statuses
            .SingleOrDefaultAsync(status => status.Id == statusId && status.Scope == scope && status.IsActive, cancellationToken);

    public Task<StatusEntity?> GetByCodeAsync(string code, string scope, CancellationToken cancellationToken) =>
        dbContext.Statuses
            .SingleOrDefaultAsync(status => status.Code == code && status.Scope == scope && status.IsActive, cancellationToken);

    public async Task<IReadOnlyList<StatusEntity>> GetActiveAsync(string scope, CancellationToken cancellationToken) =>
        await dbContext.Statuses.AsNoTracking()
            .Where(status => status.Scope == scope && status.IsActive)
            .OrderBy(status => status.Id)
            .ToListAsync(cancellationToken);
}
