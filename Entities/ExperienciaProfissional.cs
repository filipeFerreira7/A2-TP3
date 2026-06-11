namespace a2_tp3_job_connect.Entities;

public class ExperienciaProfissional : EntidadeBase
{
    public Guid ResumeId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrentJob { get; set; }

    public Curriculo Resume { get; set; } = null!;
}
