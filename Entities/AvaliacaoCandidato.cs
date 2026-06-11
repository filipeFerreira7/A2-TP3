namespace a2_tp3_job_connect.Entities;

public class AvaliacaoCandidato : EntidadeBase
{
    public Guid SelectionProcessId { get; set; }
    public Guid EvaluatorUserId { get; set; }
    public int Score { get; set; }
    public string Comments { get; set; } = string.Empty;

    public ProcessoSeletivo SelectionProcess { get; set; } = null!;
    public ApplicationUser EvaluatorUser { get; set; } = null!;
}
