using System.Text.Json;
using ITHUB.SampleAPI.Domain.Entities;
using ITHUB.SampleAPI.Persistence.Repositories;
using Microsoft.AspNetCore.Http;

namespace ITHUB.SampleAPI.Application.Services.Logging;

public sealed class LogService(
    IApplicationLogRepository logRepository,
    IHttpContextAccessor httpContextAccessor) : ILogService
{
    public Task LogAsync(
        string eventName,
        string message,
        string level = "Information",
        object? properties = null,
        CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        return logRepository.AddAsync(new ApplicationLogEntity
        {
            CreatedAtUtc = DateTime.UtcNow,
            Level = level,
            EventName = eventName,
            Message = message,
            PropertiesJson = properties is null ? null : JsonSerializer.Serialize(properties),
            TraceId = context?.TraceIdentifier,
            UserEmail = context?.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name ?? context.User.Claims.FirstOrDefault(claim => claim.Type == System.Security.Claims.ClaimTypes.Email)?.Value
                : null
        }, cancellationToken);
    }
}
