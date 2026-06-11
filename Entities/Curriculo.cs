namespace a2_tp3_job_connect.Entities;

public class Curriculo : EntidadeBase
{
    public Guid CandidateProfileId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = true;

    public PerfilCandidato CandidateProfile { get; set; } = null!;
    public ICollection<Formacao> Educations { get; set; } = [];
    public ICollection<ExperienciaProfissional> WorkExperiences { get; set; } = [];
    public ICollection<CurriculoHabilidade> Skills { get; set; } = [];
    public ICollection<DocumentoCandidato> Documents { get; set; } = [];
}
