namespace a2_tp3_job_connect.Entities;

public class StageMovementHistory : BaseEntity
{
    public Guid SelectionProcessId { get; set; }
    public Guid? FromStageId { get; set; }
    public Guid ToStageId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string? Notes { get; set; }
    public ApplicationStatus ResultingStatus { get; set; } = ApplicationStatus.InProgress;

    public SelectionProcess SelectionProcess { get; set; } = null!;
    public SelectionStage? FromStage { get; set; }
    public SelectionStage ToStage { get; set; } = null!;
    public ApplicationUser ChangedByUser { get; set; } = null!;
}
