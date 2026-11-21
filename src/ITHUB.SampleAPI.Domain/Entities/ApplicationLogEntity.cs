namespace ITHUB.SampleAPI.Domain.Entities;

public class ApplicationLogEntity
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Level { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PropertiesJson { get; set; }
    public string? TraceId { get; set; }
    public string? UserEmail { get; set; }
}
