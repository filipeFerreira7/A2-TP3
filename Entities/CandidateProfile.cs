namespace a2_tp3_job_connect.Entities;

public class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Resume? Resume { get; set; }
    public ICollection<JobApplication> Applications { get; set; } = [];
}
