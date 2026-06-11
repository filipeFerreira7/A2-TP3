namespace a2_tp3_job_connect.Entities;

public class Notificacao : EntidadeBase
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
