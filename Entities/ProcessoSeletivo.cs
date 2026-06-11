namespace a2_tp3_job_connect.Entities;

public class ProcessoSeletivo : EntidadeBase
{
    public Guid JobApplicationId { get; set; }
    public Guid CurrentStageId { get; set; }
    public bool IsFinished { get; set; }

    public Candidatura JobApplication { get; set; } = null!;
    public EtapaSelecao CurrentStage { get; set; } = null!;
    public ICollection<HistoricoMovimentoEtapa> Movements { get; set; } = [];
    public ICollection<AvaliacaoCandidato> Evaluations { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
}
