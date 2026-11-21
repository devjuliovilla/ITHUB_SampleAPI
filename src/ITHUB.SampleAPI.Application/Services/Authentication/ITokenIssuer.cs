using ITHUB.SampleAPI.Domain.Entities;

namespace ITHUB.SampleAPI.Application.Services.Authentication;

public interface ITokenIssuer
{
    (string AccessToken, DateTime ExpiresAtUtc) Issue(SampleUserEntity user);
}
