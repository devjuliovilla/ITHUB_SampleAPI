using System.ComponentModel.DataAnnotations;

namespace ITHUB.SampleAPI.Application.Contracts.Authentication;

public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(120, MinimumLength = 1)]
    public string DisplayName { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(200)]
    public string Password { get; init; } = string.Empty;
}
