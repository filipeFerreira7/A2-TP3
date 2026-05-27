namespace a2_tp3_job_connect.Entities;

public class JobSkill : BaseEntity
{
    public Guid JobPostingId { get; set; }
    public Guid SkillId { get; set; }
    public SkillRequirementType RequirementType { get; set; } = SkillRequirementType.Required;

    public JobPosting JobPosting { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
