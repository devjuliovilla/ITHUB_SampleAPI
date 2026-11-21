using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ITHUB.SampleAPI.Persistence.Repositories;

public sealed class UserRepository(SampleApiDbContext dbContext) : IUserRepository
{
    public Task<SampleUserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(SampleUserEntity user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SampleUserEntity user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
