using Microsoft.AspNetCore.Identity;

namespace a2_tp3_job_connect.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public UserPermission PrimaryPermission { get; set; } = UserPermission.Candidate;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CandidateProfile? CandidateProfile { get; set; }
    public ICollection<CompanyUser> CompanyUsers { get; set; } = [];
    public ICollection<JobPosting> CreatedJobs { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
