namespace a2_tp3_job_connect.Entities;

public class ResumeSkill : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Guid SkillId { get; set; }
    public int ProficiencyLevel { get; set; }

    public Resume Resume { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
