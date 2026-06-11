namespace a2_tp3_job_connect.Entities;

public class RegistroAuditoria : EntidadeBase
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? PreviousValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }

    public ApplicationUser? User { get; set; }
}
