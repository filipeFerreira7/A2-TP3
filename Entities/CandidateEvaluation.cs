namespace a2_tp3_job_connect.Entities;

public class CandidateEvaluation : BaseEntity
{
    public Guid SelectionProcessId { get; set; }
    public Guid EvaluatorUserId { get; set; }
    public int Score { get; set; }
    public string Comments { get; set; } = string.Empty;

    public SelectionProcess SelectionProcess { get; set; } = null!;
    public ApplicationUser EvaluatorUser { get; set; } = null!;
}
