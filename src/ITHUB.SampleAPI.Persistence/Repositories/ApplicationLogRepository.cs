using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Data;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public sealed class ApplicationLogRepository(SampleApiDbContext dbContext) : IApplicationLogRepository
{
    public async Task AddAsync(ApplicationLogEntity log, CancellationToken cancellationToken)
    {
        dbContext.ApplicationLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
