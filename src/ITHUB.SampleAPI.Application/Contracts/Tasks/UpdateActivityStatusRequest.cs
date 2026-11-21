using System.ComponentModel.DataAnnotations;

namespace ITHUB.SampleAPI.Application.Contracts.Tasks;

public sealed record UpdateActivityStatusRequest
{
    [Range(1, int.MaxValue)]
    public int StatusId { get; init; }
}
