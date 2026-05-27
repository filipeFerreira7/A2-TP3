namespace a2_tp3_job_connect.Entities;

public class Resume : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = true;

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public ICollection<Education> Educations { get; set; } = [];
    public ICollection<WorkExperience> WorkExperiences { get; set; } = [];
    public ICollection<ResumeSkill> Skills { get; set; } = [];
    public ICollection<CandidateDocument> Documents { get; set; } = [];
}
