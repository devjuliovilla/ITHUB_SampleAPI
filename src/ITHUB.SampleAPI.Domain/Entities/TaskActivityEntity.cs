namespace ITHUB.SampleAPI.Domain.Entities;

public class TaskActivityEntity
{
    public long Id { get; set; }
    public long TaskId { get; set; }
    public long CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StatusId { get; set; } = StatusCatalogConstants.ActivityPendingId;
    public StatusEntity? Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public TaskEntity? Task { get; set; }
    public SampleUserEntity? CreatedByUser { get; set; }
}
