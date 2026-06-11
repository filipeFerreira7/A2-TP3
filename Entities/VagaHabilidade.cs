namespace a2_tp3_job_connect.Entities;

public class VagaHabilidade : EntidadeBase
{
    public Guid JobPostingId { get; set; }
    public Guid SkillId { get; set; }
    public SkillRequirementType RequirementType { get; set; } = SkillRequirementType.Required;

    public Vaga JobPosting { get; set; } = null!;
    public Habilidade Skill { get; set; } = null!;
}
