namespace ITHUB.SampleAPI.Domain.Entities;

public sealed class StatusEntity
{
    public int Id { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
