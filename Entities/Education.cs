namespace a2_tp3_job_connect.Entities;

public class Education : BaseEntity
{
    public Guid ResumeId { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Resume Resume { get; set; } = null!;
}
