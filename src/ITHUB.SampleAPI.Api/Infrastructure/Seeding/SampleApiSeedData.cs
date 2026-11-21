using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ITHUB.SampleAPI.Api.Infrastructure.Seeding;

public sealed class SampleApiSeedData(
    SampleApiDbContext dbContext,
    IPasswordHasher<SampleUserEntity> passwordHasher,
    IOptions<AdminSeedOptions> adminOptions,
    IOptions<UserSeedOptions> userOptions,
    ILogger<SampleApiSeedData> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedStatusesAsync(cancellationToken);
        await SeedAdministratorAsync(cancellationToken);
        await SeedUserAsync(cancellationToken);
    }

    private async Task SeedStatusesAsync(CancellationToken cancellationToken)
    {
        var seedStatuses = new[]
        {
            CreateStatus(StatusCatalogConstants.TaskAssignedId, StatusCatalogConstants.TaskScope, StatusCatalogConstants.TaskAssigned, "Assigned"),
            CreateStatus(StatusCatalogConstants.TaskInProgressId, StatusCatalogConstants.TaskScope, StatusCatalogConstants.TaskInProgress, "In progress"),
            CreateStatus(StatusCatalogConstants.TaskCompletedId, StatusCatalogConstants.TaskScope, StatusCatalogConstants.TaskCompleted, "Completed"),
            CreateStatus(StatusCatalogConstants.ActivityPendingId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityPending, "Pending"),
            CreateStatus(StatusCatalogConstants.ActivityInProgressId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityInProgress, "In progress"),
            CreateStatus(StatusCatalogConstants.ActivityCompletedId, StatusCatalogConstants.ActivityScope, StatusCatalogConstants.ActivityCompleted, "Completed")
        };
        var existingStatuses = await dbContext.Statuses.ToListAsync(cancellationToken);
        var statusesToAdd = new List<StatusEntity>();

        foreach (var seedStatus in seedStatuses)
        {
            var existingById = existingStatuses.SingleOrDefault(status => status.Id == seedStatus.Id);
            var existingByCode = existingStatuses.SingleOrDefault(status =>
                string.Equals(status.Scope, seedStatus.Scope, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(status.Code, seedStatus.Code, StringComparison.OrdinalIgnoreCase));

            if (existingById is not null || existingByCode is not null)
            {
                if (existingById?.Id != existingByCode?.Id || existingById?.Id != seedStatus.Id)
                {
                    throw new InvalidOperationException(
                        $"Status catalog conflicts with the reserved seed value {seedStatus.Scope}/{seedStatus.Code} (ID {seedStatus.Id}).");
                }

                continue;
            }

            statusesToAdd.Add(seedStatus);
        }

        if (statusesToAdd.Count == 0)
            return;

        dbContext.Statuses.AddRange(statusesToAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {StatusCount} task/activity statuses.", statusesToAdd.Count);
    }

    private async Task SeedAdministratorAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(user => user.Role == UserRoles.Admin, cancellationToken))
            return;

        var options = adminOptions.Value;
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "No administrator exists. Configure SeedData:Administrator:Email and SeedData:Administrator:Password to seed one.");
        }

        var email = options.Email.Trim().ToLowerInvariant();
        var administrator = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        if (administrator is null)
        {
            administrator = new SampleUserEntity
            {
                Email = email,
                DisplayName = string.IsNullOrWhiteSpace(options.DisplayName)
                    ? "Demo Administrator"
                    : options.DisplayName.Trim(),
                Role = UserRoles.Admin,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };
            administrator.PasswordHash = passwordHasher.HashPassword(administrator, options.Password);
            dbContext.Users.Add(administrator);
        }
        else
        {
            administrator.Role = UserRoles.Admin;
            administrator.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded demo administrator account {AdminEmail}.", email);
    }

    private async Task SeedUserAsync(CancellationToken cancellationToken)
    {
        var options = userOptions.Value;
        if (string.IsNullOrWhiteSpace(options.Email) && string.IsNullOrWhiteSpace(options.Password))
            return;
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Configure both SeedData:User:Email and SeedData:User:Password, or leave both empty.");
        }

        var email = options.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
            return;

        var user = new SampleUserEntity
        {
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(options.DisplayName) ? "Demo User" : options.DisplayName.Trim(),
            Role = UserRoles.User,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, options.Password);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded demo user account {UserEmail}.", email);
    }

    private static StatusEntity CreateStatus(int id, string scope, string code, string name) => new()
    {
        Id = id,
        Scope = scope,
        Code = code,
        Name = name,
        IsActive = true
    };
}
