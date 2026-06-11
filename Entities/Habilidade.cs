namespace a2_tp3_job_connect.Entities;

public class Habilidade : EntidadeBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<VagaHabilidade> JobSkills { get; set; } = [];
    public ICollection<CurriculoHabilidade> ResumeSkills { get; set; } = [];
}
