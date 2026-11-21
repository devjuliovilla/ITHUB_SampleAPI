using ITHUB.SampleAPI.Domain.Entities;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public interface IApplicationLogRepository
{
    Task AddAsync(ApplicationLogEntity log, CancellationToken cancellationToken);
}
