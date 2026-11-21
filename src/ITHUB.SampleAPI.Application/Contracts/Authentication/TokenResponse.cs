namespace ITHUB.SampleAPI.Application.Contracts.Authentication;

public sealed record TokenResponse(long UserId, string AccessToken, DateTime ExpiresAtUtc, string Email, string DisplayName);
