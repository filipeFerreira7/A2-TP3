namespace a2_tp3_job_connect.Entities;

public class AprovacaoVaga : EntidadeBase
{
    public Guid JobPostingId { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public bool Approved { get; set; }
    public string? Notes { get; set; }

    public Vaga JobPosting { get; set; } = null!;
    public ApplicationUser ApprovedByUser { get; set; } = null!;
}
