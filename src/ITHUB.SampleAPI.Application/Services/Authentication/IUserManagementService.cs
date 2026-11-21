using ITHUB.SampleAPI.Application.Contracts.Authentication;

namespace ITHUB.SampleAPI.Application.Services.Authentication;

public interface IUserManagementService
{
    Task<CreatedUserDto?> CreateUserAsync(RegisterRequest request, CancellationToken cancellationToken);
}
