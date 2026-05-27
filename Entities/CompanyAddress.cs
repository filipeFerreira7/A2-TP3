namespace a2_tp3_job_connect.Entities;

public class CompanyAddress : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string ZipCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public bool ValidatedByViaCep { get; set; }
    public DateTime? ValidatedAt { get; set; }

    public Company Company { get; set; } = null!;
}
