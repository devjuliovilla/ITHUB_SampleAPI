using ITHUB.SampleAPI.Domain.Entities;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public interface IUserRepository
{
    Task<SampleUserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(SampleUserEntity user, CancellationToken cancellationToken);
    Task UpdateAsync(SampleUserEntity user, CancellationToken cancellationToken);
}
