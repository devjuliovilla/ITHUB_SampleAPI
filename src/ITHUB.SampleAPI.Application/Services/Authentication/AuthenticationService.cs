using ITHUB.SampleAPI.Application.Contracts.Authentication;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ITHUB.SampleAPI.Application.Services.Authentication;

public sealed class AuthenticationService(
    IUserRepository userRepository,
    IPasswordHasher<SampleUserEntity> passwordHasher,
    ITokenIssuer tokenIssuer) : IAuthenticationService
{
    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return null;

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return CreateTokenResponse(user);
    }

    private TokenResponse CreateTokenResponse(SampleUserEntity user)
    {
        var token = tokenIssuer.Issue(user);
        return new TokenResponse(user.Id, token.AccessToken, token.ExpiresAtUtc, user.Email, user.DisplayName);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
