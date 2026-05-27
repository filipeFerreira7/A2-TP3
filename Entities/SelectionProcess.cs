namespace a2_tp3_job_connect.Entities;

public class SelectionProcess : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public Guid CurrentStageId { get; set; }
    public bool IsFinished { get; set; }

    public JobApplication JobApplication { get; set; } = null!;
    public SelectionStage CurrentStage { get; set; } = null!;
    public ICollection<StageMovementHistory> Movements { get; set; } = [];
    public ICollection<CandidateEvaluation> Evaluations { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
}
