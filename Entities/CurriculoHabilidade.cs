namespace a2_tp3_job_connect.Entities;

public class CurriculoHabilidade : EntidadeBase
{
    public Guid ResumeId { get; set; }
    public Guid SkillId { get; set; }
    public int ProficiencyLevel { get; set; }

    public Curriculo Resume { get; set; } = null!;
    public Habilidade Skill { get; set; } = null!;
}
