namespace a2_tp3_job_connect.Entities;

public class JobPosting : BaseEntity
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

    public Company Company { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<JobSkill> Skills { get; set; } = [];
    public ICollection<JobApplication> Applications { get; set; } = [];
    public ICollection<JobApproval> Approvals { get; set; } = [];
}
