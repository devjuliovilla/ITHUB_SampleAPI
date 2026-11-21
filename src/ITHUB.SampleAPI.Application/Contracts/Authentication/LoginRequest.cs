using System.ComponentModel.DataAnnotations;

namespace ITHUB.SampleAPI.Application.Contracts.Authentication;

public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(1), MaxLength(200)]
    public string Password { get; init; } = string.Empty;
}
