namespace a2_tp3_job_connect.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<JobSkill> JobSkills { get; set; } = [];
    public ICollection<ResumeSkill> ResumeSkills { get; set; } = [];
}
