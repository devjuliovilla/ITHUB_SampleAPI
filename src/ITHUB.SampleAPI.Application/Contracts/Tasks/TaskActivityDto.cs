namespace ITHUB.SampleAPI.Application.Contracts.Tasks;

public sealed record TaskActivityDto(
    long Id,
    long TaskId,
    long CreatedByUserId,
    string Title,
    string Description,
    StatusDto Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
