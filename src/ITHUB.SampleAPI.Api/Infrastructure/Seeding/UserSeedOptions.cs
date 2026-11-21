namespace ITHUB.SampleAPI.Api.Infrastructure.Seeding;

public sealed class UserSeedOptions
{
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
