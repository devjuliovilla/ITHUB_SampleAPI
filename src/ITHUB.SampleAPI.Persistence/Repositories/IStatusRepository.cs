using ITHUB.SampleAPI.Domain.Entities;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public interface IStatusRepository
{
    Task<StatusEntity?> GetByIdAsync(int statusId, string scope, CancellationToken cancellationToken);
    Task<StatusEntity?> GetByCodeAsync(string code, string scope, CancellationToken cancellationToken);
    Task<IReadOnlyList<StatusEntity>> GetActiveAsync(string scope, CancellationToken cancellationToken);
}
