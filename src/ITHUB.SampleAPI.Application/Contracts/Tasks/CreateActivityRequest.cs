using System.ComponentModel.DataAnnotations;

namespace ITHUB.SampleAPI.Application.Contracts.Tasks;

public sealed record CreateActivityRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
