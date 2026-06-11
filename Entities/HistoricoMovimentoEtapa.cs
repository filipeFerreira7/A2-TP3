namespace a2_tp3_job_connect.Entities;

public class HistoricoMovimentoEtapa : EntidadeBase
{
    public Guid SelectionProcessId { get; set; }
    public Guid? FromStageId { get; set; }
    public Guid ToStageId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string? Notes { get; set; }
    public ApplicationStatus ResultingStatus { get; set; } = ApplicationStatus.InProgress;

    public ProcessoSeletivo SelectionProcess { get; set; } = null!;
    public EtapaSelecao? FromStage { get; set; }
    public EtapaSelecao ToStage { get; set; } = null!;
    public ApplicationUser ChangedByUser { get; set; } = null!;
}
