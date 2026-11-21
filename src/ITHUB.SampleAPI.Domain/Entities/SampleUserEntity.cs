namespace ITHUB.SampleAPI.Domain.Entities;

public class SampleUserEntity
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.User;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; }
}
