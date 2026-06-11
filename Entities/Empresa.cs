namespace a2_tp3_job_connect.Entities;

public class Empresa : EntidadeBase
{
    public string LegalName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public EnderecoEmpresa? Address { get; set; }
    public ICollection<UsuarioEmpresa> Users { get; set; } = [];
    public ICollection<Vaga> Jobs { get; set; } = [];
}
