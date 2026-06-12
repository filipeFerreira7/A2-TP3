using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Data;

public class JobConnectDbContext(DbContextOptions<JobConnectDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<EnderecoEmpresa> EnderecosEmpresa => Set<EnderecoEmpresa>();
    public DbSet<UsuarioEmpresa> UsuariosEmpresa => Set<UsuarioEmpresa>();
    public DbSet<Habilidade> Habilidades => Set<Habilidade>();
    public DbSet<Vaga> Vagas => Set<Vaga>();
    public DbSet<VagaHabilidade> VagasHabilidades => Set<VagaHabilidade>();
    public DbSet<PerfilCandidato> PerfisCandidatos => Set<PerfilCandidato>();
    public DbSet<Curriculo> Curriculos => Set<Curriculo>();
    public DbSet<Formacao> Formacoes => Set<Formacao>();
    public DbSet<ExperienciaProfissional> ExperienciasProfissionais => Set<ExperienciaProfissional>();
    public DbSet<CurriculoHabilidade> CurriculosHabilidades => Set<CurriculoHabilidade>();
    public DbSet<DocumentoCandidato> DocumentosCandidatos => Set<DocumentoCandidato>();
    public DbSet<Candidatura> Candidaturas => Set<Candidatura>();
    public DbSet<ProcessoSeletivo> ProcessosSeletivos => Set<ProcessoSeletivo>();
    public DbSet<EtapaSelecao> EtapasSelecao => Set<EtapaSelecao>();
    public DbSet<HistoricoMovimentoEtapa> HistoricosMovimentosEtapas => Set<HistoricoMovimentoEtapa>();
    public DbSet<AvaliacaoCandidato> AvaliacoesCandidatos => Set<AvaliacaoCandidato>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<AprovacaoVaga> AprovacoesVagas => Set<AprovacaoVaga>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentity(builder);
        ConfigureCompany(builder);
        ConfigureJobs(builder);
        ConfigureCandidate(builder);
        ConfigureSelectionProcess(builder);
        ConfigureSupportTables(builder);
        ConfigureSoftDeleteFilters(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditDates();
        return base.SaveChanges();
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PrimaryPermission).HasConversion<string>().HasMaxLength(32);
        });
    }

    private static void ConfigureCompany(ModelBuilder builder)
    {
        builder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresas");
            entity.HasIndex(x => x.Cnpj).IsUnique();
            entity.Property(x => x.LegalName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.TradeName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Cnpj).HasMaxLength(14).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(25);
            entity.Property(x => x.LinkedInUrl).HasMaxLength(300);
            entity.Property(x => x.Description).HasMaxLength(4000);
        });

        builder.Entity<EnderecoEmpresa>(entity =>
        {
            entity.ToTable("EnderecosEmpresa");
            entity.HasIndex(x => x.CompanyId).IsUnique();
            entity.Property(x => x.ZipCode).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Street).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Complement).HasMaxLength(100);
            entity.Property(x => x.District).HasMaxLength(100).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
            entity.Property(x => x.State).HasMaxLength(2).IsRequired();
            entity.HasOne(x => x.Company).WithOne(x => x.Address).HasForeignKey<EnderecoEmpresa>(x => x.CompanyId);
        });

        builder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.ToTable("UsuariosEmpresa");
            entity.HasIndex(x => new { x.CompanyId, x.UserId, x.Role }).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
            entity.HasOne(x => x.Company).WithMany(x => x.Users).HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.User).WithMany(x => x.CompanyUsers).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureJobs(ModelBuilder builder)
    {
        builder.Entity<Habilidade>(entity =>
        {
            entity.ToTable("Habilidades");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(300);
        });

        builder.Entity<Vaga>(entity =>
        {
            entity.ToTable("Vagas");
            entity.HasIndex(x => new { x.CompanyId, x.Title, x.ClosingDate }).IsUnique();
            entity.Property(x => x.Title).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.MinimumSalary).HasColumnType("decimal(12,2)");
            entity.Property(x => x.MaximumSalary).HasColumnType("decimal(12,2)");
            entity.Property(x => x.WorkModel).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Tags).HasMaxLength(500);
            entity.HasOne(x => x.Company).WithMany(x => x.Jobs).HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.CreatedByUser).WithMany(x => x.CreatedJobs).HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<VagaHabilidade>(entity =>
        {
            entity.ToTable("VagasHabilidades");
            entity.HasIndex(x => new { x.JobPostingId, x.SkillId }).IsUnique();
            entity.Property(x => x.RequirementType).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.JobPosting).WithMany(x => x.Skills).HasForeignKey(x => x.JobPostingId);
            entity.HasOne(x => x.Skill).WithMany(x => x.JobSkills).HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AprovacaoVaga>(entity =>
        {
            entity.ToTable("AprovacoesVagas");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasOne(x => x.JobPosting).WithMany(x => x.Approvals).HasForeignKey(x => x.JobPostingId);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCandidate(ModelBuilder builder)
    {
        builder.Entity<PerfilCandidato>(entity =>
        {
            entity.ToTable("PerfisCandidatos");
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.Cpf).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Cpf).HasMaxLength(11).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(25);
            entity.Property(x => x.LinkedInUrl).HasMaxLength(300);
            entity.Property(x => x.PortfolioUrl).HasMaxLength(300);
            entity.HasOne(x => x.User).WithOne(x => x.CandidateProfile).HasForeignKey<PerfilCandidato>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Curriculo>(entity =>
        {
            entity.ToTable("Curriculos");
            entity.HasIndex(x => x.CandidateProfileId).IsUnique();
            entity.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.CandidateProfile).WithOne(x => x.Resume).HasForeignKey<Curriculo>(x => x.CandidateProfileId);
        });

        builder.Entity<Formacao>(entity =>
        {
            entity.ToTable("Formacoes");
            entity.Property(x => x.Institution).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Course).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Degree).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Resume).WithMany(x => x.Educations).HasForeignKey(x => x.ResumeId);
        });

        builder.Entity<ExperienciaProfissional>(entity =>
        {
            entity.ToTable("ExperienciasProfissionais");
            entity.Property(x => x.CompanyName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Position).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.Resume).WithMany(x => x.WorkExperiences).HasForeignKey(x => x.ResumeId);
        });

        builder.Entity<CurriculoHabilidade>(entity =>
        {
            entity.ToTable("CurriculosHabilidades");
            entity.HasIndex(x => new { x.ResumeId, x.SkillId }).IsUnique();
            entity.HasOne(x => x.Resume).WithMany(x => x.Skills).HasForeignKey(x => x.ResumeId);
            entity.HasOne(x => x.Skill).WithMany(x => x.ResumeSkills).HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DocumentoCandidato>(entity =>
        {
            entity.ToTable("DocumentosCandidatos");
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.FileName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Resume).WithMany(x => x.Documents).HasForeignKey(x => x.ResumeId);
        });

        builder.Entity<Candidatura>(entity =>
        {
            entity.ToTable("Candidaturas");
            entity.HasIndex(x => new { x.JobPostingId, x.CandidateProfileId }).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.AvailabilityPreference).HasMaxLength(30);
            entity.Property(x => x.SalaryExpectation).HasMaxLength(500);
            entity.Property(x => x.ExperienceNotes).HasMaxLength(3000);
            entity.HasOne(x => x.JobPosting).WithMany(x => x.Applications).HasForeignKey(x => x.JobPostingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CandidateProfile).WithMany(x => x.Applications).HasForeignKey(x => x.CandidateProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Resume).WithMany().HasForeignKey(x => x.ResumeId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSelectionProcess(ModelBuilder builder)
    {
        builder.Entity<EtapaSelecao>(entity =>
        {
            entity.ToTable("EtapasSelecao");
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProcessoSeletivo>(entity =>
        {
            entity.ToTable("ProcessosSeletivos");
            entity.HasIndex(x => x.JobApplicationId).IsUnique();
            entity.HasOne(x => x.JobApplication).WithOne(x => x.SelectionProcess).HasForeignKey<ProcessoSeletivo>(x => x.JobApplicationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CurrentStage).WithMany(x => x.CurrentProcesses).HasForeignKey(x => x.CurrentStageId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<HistoricoMovimentoEtapa>(entity =>
        {
            entity.ToTable("HistoricosMovimentosEtapas");
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.ResultingStatus).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.SelectionProcess).WithMany(x => x.Movements).HasForeignKey(x => x.SelectionProcessId);
            entity.HasOne(x => x.FromStage).WithMany(x => x.FromMovements).HasForeignKey(x => x.FromStageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToStage).WithMany(x => x.ToMovements).HasForeignKey(x => x.ToStageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AvaliacaoCandidato>(entity =>
        {
            entity.ToTable("AvaliacoesCandidatos");
            entity.Property(x => x.Comments).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.SelectionProcess).WithMany(x => x.Evaluations).HasForeignKey(x => x.SelectionProcessId);
            entity.HasOne(x => x.EvaluatorUser).WithMany().HasForeignKey(x => x.EvaluatorUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Feedback>(entity =>
        {
            entity.ToTable("Feedbacks");
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.SelectionProcess).WithMany(x => x.Feedbacks).HasForeignKey(x => x.SelectionProcessId);
            entity.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSupportTables(ModelBuilder builder)
    {
        builder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("Notificacoes");
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Title).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RegistroAuditoria>(entity =>
        {
            entity.ToTable("RegistrosAuditoria");
            entity.Property(x => x.Action).HasMaxLength(120).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.PreviousValues).HasColumnType("nvarchar(max)");
            entity.Property(x => x.NewValues).HasColumnType("nvarchar(max)");
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSoftDeleteFilters(ModelBuilder builder)
    {
        builder.Entity<Empresa>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<EnderecoEmpresa>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<UsuarioEmpresa>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Habilidade>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Vaga>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<VagaHabilidade>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<PerfilCandidato>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Curriculo>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Formacao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ExperienciaProfissional>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CurriculoHabilidade>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<DocumentoCandidato>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Candidatura>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ProcessoSeletivo>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<EtapaSelecao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<HistoricoMovimentoEtapa>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AvaliacaoCandidato>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Feedback>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Notificacao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<RegistroAuditoria>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AprovacaoVaga>().HasQueryFilter(x => !x.IsDeleted);
    }

    private void ApplyAuditDates()
    {
        foreach (var entry in ChangeTracker.Entries<EntidadeBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
