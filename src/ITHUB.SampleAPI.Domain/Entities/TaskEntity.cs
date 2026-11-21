namespace ITHUB.SampleAPI.Domain.Entities;

public class TaskEntity
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long AssignedUserId { get; set; }
    public long CreatedByUserId { get; set; }
    public int StatusId { get; set; } = StatusCatalogConstants.TaskAssignedId;
    public StatusEntity? Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public SampleUserEntity? AssignedUser { get; set; }
    public SampleUserEntity? CreatedByUser { get; set; }
    public ICollection<TaskActivityEntity> Activities { get; set; } = new List<TaskActivityEntity>();
}
