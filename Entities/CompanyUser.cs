namespace a2_tp3_job_connect.Entities;

public class CompanyUser : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public CompanyUserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
