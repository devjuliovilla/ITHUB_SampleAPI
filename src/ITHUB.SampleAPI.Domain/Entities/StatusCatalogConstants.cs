namespace ITHUB.SampleAPI.Domain.Entities;

public static class StatusCatalogConstants
{
    public const string TaskScope = "Task";
    public const string ActivityScope = "Activity";

    public const string TaskAssigned = "Assigned";
    public const string TaskInProgress = "InProgress";
    public const string TaskCompleted = "Completed";

    public const string ActivityPending = "Pending";
    public const string ActivityInProgress = "InProgress";
    public const string ActivityCompleted = "Completed";

    public const int TaskAssignedId = 1;
    public const int TaskInProgressId = 2;
    public const int TaskCompletedId = 3;
    public const int ActivityPendingId = 4;
    public const int ActivityInProgressId = 5;
    public const int ActivityCompletedId = 6;
}
