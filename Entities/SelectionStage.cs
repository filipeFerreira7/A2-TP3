namespace a2_tp3_job_connect.Entities;

public class SelectionStage : BaseEntity
{
    public Guid? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsDefaultInitialStage { get; set; }

    public Company? Company { get; set; }
    public ICollection<SelectionProcess> CurrentProcesses { get; set; } = [];
    public ICollection<StageMovementHistory> FromMovements { get; set; } = [];
    public ICollection<StageMovementHistory> ToMovements { get; set; } = [];
}
