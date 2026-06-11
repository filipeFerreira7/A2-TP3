namespace a2_tp3_job_connect.Entities;

public class Vaga : EntidadeBase
{
    public Guid CompanyId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public WorkModel WorkModel { get; set; }
    public JobLevel Level { get; set; }
    public int OpenPositions { get; set; }
    public JobStatus Status { get; set; } = JobStatus.PendingApproval;
    public DateTime? PublishedAt { get; set; }
    public DateTime ClosingDate { get; set; }
    public string? Tags { get; set; }
    public string? Benefits { get; set; }
    public string? Location { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? Schedule { get; set; }

    public Empresa Company { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<VagaHabilidade> Skills { get; set; } = [];
    public ICollection<Candidatura> Applications { get; set; } = [];
    public ICollection<AprovacaoVaga> Approvals { get; set; } = [];
}
