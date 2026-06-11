using Microsoft.AspNetCore.Identity;

namespace a2_tp3_job_connect.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public UserPermission PrimaryPermission { get; set; } = UserPermission.Candidate;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PerfilCandidato? CandidateProfile { get; set; }
    public ICollection<UsuarioEmpresa> CompanyUsers { get; set; } = [];
    public ICollection<Vaga> CreatedJobs { get; set; } = [];
    public ICollection<Notificacao> Notifications { get; set; } = [];
    public ICollection<RegistroAuditoria> AuditLogs { get; set; } = [];
}
