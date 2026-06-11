namespace a2_tp3_job_connect.Entities;

public class Candidatura : EntidadeBase
{
    public Guid JobPostingId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public Guid ResumeId { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Received;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string? AvailabilityPreference { get; set; }
    public string? SalaryExpectation { get; set; }
    public string? ExperienceNotes { get; set; }

    public Vaga JobPosting { get; set; } = null!;
    public PerfilCandidato CandidateProfile { get; set; } = null!;
    public Curriculo Resume { get; set; } = null!;
    public ProcessoSeletivo? SelectionProcess { get; set; }
}
