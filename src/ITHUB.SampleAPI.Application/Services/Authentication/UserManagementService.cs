using ITHUB.SampleAPI.Application.Contracts.Authentication;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ITHUB.SampleAPI.Application.Services.Authentication;

public sealed class UserManagementService(
    IUserRepository userRepository,
    IPasswordHasher<SampleUserEntity> passwordHasher) : IUserManagementService
{
    public async Task<CreatedUserDto?> CreateUserAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await userRepository.FindByEmailAsync(email, cancellationToken) is not null)
            return null;

        var user = new SampleUserEntity
        {
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRoles.User,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await userRepository.AddAsync(user, cancellationToken);
        return new CreatedUserDto(user.Id, user.Email, user.DisplayName, user.Role);
    }
}
