namespace ITHUB.SampleAPI.Application.Services.Logging;

public interface ILogService
{
    Task LogAsync(
        string eventName,
        string message,
        string level = "Information",
        object? properties = null,
        CancellationToken cancellationToken = default);
}
