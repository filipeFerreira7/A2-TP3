namespace a2_tp3_job_connect.Entities;

public class Feedback : BaseEntity
{
    public Guid SelectionProcessId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAutomatic { get; set; }
    public DateTime? SentAt { get; set; }

    public SelectionProcess SelectionProcess { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
