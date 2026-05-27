namespace a2_tp3_job_connect.Entities;

public class JobApplication : BaseEntity
{
    public Guid JobPostingId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public Guid ResumeId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Received;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public JobPosting JobPosting { get; set; } = null!;
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Resume Resume { get; set; } = null!;
    public SelectionProcess? SelectionProcess { get; set; }
}
