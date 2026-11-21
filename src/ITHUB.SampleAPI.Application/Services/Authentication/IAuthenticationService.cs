using ITHUB.SampleAPI.Application.Contracts.Authentication;

namespace ITHUB.SampleAPI.Application.Services.Authentication;

public interface IAuthenticationService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
