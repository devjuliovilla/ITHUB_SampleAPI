using ITHUB.SampleAPI.Application.Contracts.Authentication;
using ITHUB.SampleAPI.Application.Services.Authentication;
using ITHUB.SampleAPI.Api.Controllers;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace ITHUB.SampleAPI.UnitTests;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task UserCreatedByAdminCanLogin_UsesNormalizedEmailAndHashedPassword()
    {
        var repository = new InMemoryUserRepository();
        var userManagementService = new UserManagementService(repository, new PasswordHasher<SampleUserEntity>());

        var createdUser = await userManagementService.CreateUserAsync(new RegisterRequest
        {
            Email = "  Demo@Example.com ",
            DisplayName = "Demo User",
            Password = "correct-horse-battery"
        }, CancellationToken.None);
        var authService = CreateService(repository);
        var loggedIn = await authService.LoginAsync(new LoginRequest
        {
            Email = "DEMO@example.com",
            Password = "correct-horse-battery"
        }, CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.NotNull(loggedIn);
        Assert.Equal("demo@example.com", loggedIn.Email);
        var savedUser = Assert.IsType<SampleUserEntity>(repository.User);
        Assert.Equal(UserRoles.User, savedUser.Role);
        Assert.NotEqual("correct-horse-battery", savedUser.PasswordHash);
        Assert.Equal("demo-token", loggedIn.AccessToken);
    }

    [Fact]
    public async Task UserManagementService_CreatesUserRoleAccountWithHashedPassword()
    {
        var repository = new InMemoryUserRepository();
        var service = new UserManagementService(repository, new PasswordHasher<SampleUserEntity>());

        var createdUser = await service.CreateUserAsync(new RegisterRequest
        {
            Email = " New.User@example.com ",
            DisplayName = "New User",
            Password = "a-strong-demo-password"
        }, CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.Equal("new.user@example.com", createdUser.Email);
        Assert.Equal(UserRoles.User, createdUser.Role);
        Assert.NotEqual("a-strong-demo-password", repository.User!.PasswordHash);
    }

    [Fact]
    public async Task UserManagementService_WhenEmailAlreadyExists_ReturnsNull()
    {
        var service = new UserManagementService(
            new InMemoryUserRepository(),
            new PasswordHasher<SampleUserEntity>());
        var request = new RegisterRequest
        {
            Email = "demo@example.com",
            DisplayName = "Demo User",
            Password = "correct-horse-battery"
        };

        Assert.NotNull(await service.CreateUserAsync(request, CancellationToken.None));
        Assert.Null(await service.CreateUserAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ReturnsNull()
    {
        var repository = new InMemoryUserRepository();
        var userManagementService = new UserManagementService(repository, new PasswordHasher<SampleUserEntity>());
        var service = CreateService(repository);
        await userManagementService.CreateUserAsync(new RegisterRequest
        {
            Email = "demo@example.com",
            DisplayName = "Demo User",
            Password = "correct-horse-battery"
        }, CancellationToken.None);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "demo@example.com",
            Password = "wrong-password"
        }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UserManagementService_CreatesAccountWithUserRoleAndHashedPassword()
    {
        var repository = new InMemoryUserRepository();
        var service = new UserManagementService(repository, new PasswordHasher<SampleUserEntity>());

        var createdUser = await service.CreateUserAsync(new RegisterRequest
        {
            Email = "new.user@example.com",
            DisplayName = "New User",
            Password = "strong-demo-password"
        }, CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.Equal(UserRoles.User, createdUser.Role);
        Assert.NotEqual("strong-demo-password", repository.User!.PasswordHash);
        Assert.Equal("new.user@example.com", createdUser.Email);
    }

    [Fact]
    public void UsersController_RequiresAdministratorRole()
    {
        var createUserMethod = typeof(UsersController).GetMethod(nameof(UsersController.CreateUser));
        var authorization = createUserMethod is null
            ? null
            : Attribute.GetCustomAttribute(createUserMethod, typeof(AuthorizeAttribute)) as AuthorizeAttribute;

        Assert.NotNull(authorization);
        Assert.Equal(UserRoles.Admin, authorization.Roles);
    }

    private static AuthenticationService CreateService(InMemoryUserRepository repository) =>
        new(repository, new PasswordHasher<SampleUserEntity>(), new TestTokenIssuer());

    private sealed class TestTokenIssuer : ITokenIssuer
    {
        public (string AccessToken, DateTime ExpiresAtUtc) Issue(SampleUserEntity user) =>
            ("demo-token", DateTime.UtcNow.AddMinutes(60));
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public SampleUserEntity? User { get; private set; }

        public Task<SampleUserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(User?.Email == email ? User : null);

        public Task AddAsync(SampleUserEntity user, CancellationToken cancellationToken)
        {
            user.Id = 1;
            User = user;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SampleUserEntity user, CancellationToken cancellationToken)
        {
            User = user;
            return Task.CompletedTask;
        }
    }
}
