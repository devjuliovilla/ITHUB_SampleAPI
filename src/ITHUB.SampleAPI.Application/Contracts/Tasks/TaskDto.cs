namespace ITHUB.SampleAPI.Application.Contracts.Tasks;

public sealed record TaskDto(
    long Id,
    string Title,
    string Description,
    long AssignedUserId,
    long CreatedByUserId,
    StatusDto Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<TaskActivityDto> Activities);
