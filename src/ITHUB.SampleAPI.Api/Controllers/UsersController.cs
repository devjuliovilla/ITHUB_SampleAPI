using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ITHUB.SampleAPI.Application.Contracts.Authentication;
using ITHUB.SampleAPI.Application.Services.Authentication;
using ITHUB.SampleAPI.Application.Services.Logging;
using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHUB.SampleAPI.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(
    IUserManagementService userManagementService,
    ILogService logService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType<CreatedUserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await userManagementService.CreateUserAsync(request, cancellationToken);
        return user is null
            ? Conflict(new { message = "An account with that email already exists." })
            : StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpGet("me")]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!long.TryParse(subject, out var userId))
            return Unauthorized();

        var email = User.FindFirstValue(ClaimTypes.Email);
        var displayName = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        await logService.LogAsync(
            "users.me.read",
            "Authenticated user profile was requested.",
            cancellationToken: cancellationToken);

        return Ok(new CurrentUserDto(userId, email ?? string.Empty, displayName ?? string.Empty, role ?? string.Empty));
    }
}

public sealed record CurrentUserDto(long UserId, string Email, string DisplayName, string Role);
