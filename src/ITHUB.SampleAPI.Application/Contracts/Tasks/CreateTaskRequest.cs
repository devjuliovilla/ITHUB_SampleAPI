using System.ComponentModel.DataAnnotations;

namespace ITHUB.SampleAPI.Application.Contracts.Tasks;

public sealed record CreateTaskRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long AssignedUserId { get; init; }
}
