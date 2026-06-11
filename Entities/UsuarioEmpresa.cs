namespace a2_tp3_job_connect.Entities;

public class UsuarioEmpresa : EntidadeBase
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public CompanyUserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Empresa Company { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
