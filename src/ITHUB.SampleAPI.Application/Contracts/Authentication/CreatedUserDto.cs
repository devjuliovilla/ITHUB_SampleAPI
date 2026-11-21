namespace ITHUB.SampleAPI.Application.Contracts.Authentication;

public sealed record CreatedUserDto(long UserId, string Email, string DisplayName, string Role);
