namespace a2_tp3_job_connect.Entities;

public class EtapaSelecao : EntidadeBase
{
    public Guid? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsDefaultInitialStage { get; set; }

    public Empresa? Company { get; set; }
    public ICollection<ProcessoSeletivo> CurrentProcesses { get; set; } = [];
    public ICollection<HistoricoMovimentoEtapa> FromMovements { get; set; } = [];
    public ICollection<HistoricoMovimentoEtapa> ToMovements { get; set; } = [];
}
