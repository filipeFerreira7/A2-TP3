namespace a2_tp3_job_connect.Entities;

public class Formacao : EntidadeBase
{
    public Guid ResumeId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Curriculo Resume { get; set; } = null!;
}
