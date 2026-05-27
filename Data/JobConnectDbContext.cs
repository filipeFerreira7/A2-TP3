using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Data;

public class JobConnectDbContext(DbContextOptions<JobConnectDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyAddress> CompanyAddresses => Set<CompanyAddress>();
    public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<ResumeSkill> ResumeSkills => Set<ResumeSkill>();
    public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<SelectionProcess> SelectionProcesses => Set<SelectionProcess>();
    public DbSet<SelectionStage> SelectionStages => Set<SelectionStage>();
    public DbSet<StageMovementHistory> StageMovementHistories => Set<StageMovementHistory>();
    public DbSet<CandidateEvaluation> CandidateEvaluations => Set<CandidateEvaluation>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<JobApproval> JobApprovals => Set<JobApproval>();

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
        builder.Entity<Company>(entity =>
        {
            entity.HasIndex(x => x.Cnpj).IsUnique();
            entity.Property(x => x.LegalName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.TradeName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Cnpj).HasMaxLength(14).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(25);
            entity.Property(x => x.LinkedInUrl).HasMaxLength(300);
        });

        builder.Entity<CompanyAddress>(entity =>
        {
            entity.HasIndex(x => x.CompanyId).IsUnique();
            entity.Property(x => x.ZipCode).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Street).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Number).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Complement).HasMaxLength(100);
            entity.Property(x => x.District).HasMaxLength(100).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100).IsRequired();
            entity.Property(x => x.State).HasMaxLength(2).IsRequired();
            entity.HasOne(x => x.Company).WithOne(x => x.Address).HasForeignKey<CompanyAddress>(x => x.CompanyId);
        });

        builder.Entity<CompanyUser>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.UserId, x.Role }).IsUnique();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
            entity.HasOne(x => x.Company).WithMany(x => x.Users).HasForeignKey(x => x.CompanyId);
            entity.HasOne(x => x.User).WithMany(x => x.CompanyUsers).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureJobs(ModelBuilder builder)
    {
        builder.Entity<Skill>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(300);
        });

        builder.Entity<JobPosting>(entity =>
        {
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

        builder.Entity<JobSkill>(entity =>
        {
            entity.HasIndex(x => new { x.JobPostingId, x.SkillId }).IsUnique();
            entity.Property(x => x.RequirementType).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.JobPosting).WithMany(x => x.Skills).HasForeignKey(x => x.JobPostingId);
            entity.HasOne(x => x.Skill).WithMany(x => x.JobSkills).HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<JobApproval>(entity =>
        {
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasOne(x => x.JobPosting).WithMany(x => x.Approvals).HasForeignKey(x => x.JobPostingId);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCandidate(ModelBuilder builder)
    {
        builder.Entity<CandidateProfile>(entity =>
        {
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.Cpf).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Cpf).HasMaxLength(11).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(25);
            entity.Property(x => x.LinkedInUrl).HasMaxLength(300);
            entity.Property(x => x.PortfolioUrl).HasMaxLength(300);
            entity.HasOne(x => x.User).WithOne(x => x.CandidateProfile).HasForeignKey<CandidateProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Resume>(entity =>
        {
            entity.HasIndex(x => x.CandidateProfileId).IsUnique();
            entity.Property(x => x.Summary).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.CandidateProfile).WithOne(x => x.Resume).HasForeignKey<Resume>(x => x.CandidateProfileId);
        });

        builder.Entity<Education>(entity =>
        {
            entity.Property(x => x.Institution).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Course).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Degree).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Resume).WithMany(x => x.Educations).HasForeignKey(x => x.ResumeId);
        });

        builder.Entity<WorkExperience>(entity =>
        {
            entity.Property(x => x.CompanyName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Position).HasMaxLength(140).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.Resume).WithMany(x => x.WorkExperiences).HasForeignKey(x => x.ResumeId);
        });

        builder.Entity<ResumeSkill>(entity =>
        {
            entity.HasIndex(x => new { x.ResumeId, x.SkillId }).IsUnique();
            entity.HasOne(x => x.Resume).WithMany(x => x.Skills).HasForeignKey(x => x.ResumeId);
            entity.HasOne(x => x.Skill).WithMany(x => x.ResumeSkills).HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CandidateDocument>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.FileName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Resume).WithMany(x => x.Documents).HasForeignKey(x => x.ResumeId);
        });

        builder.Entity<JobApplication>(entity =>
        {
            entity.HasIndex(x => new { x.JobPostingId, x.CandidateProfileId }).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.JobPosting).WithMany(x => x.Applications).HasForeignKey(x => x.JobPostingId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CandidateProfile).WithMany(x => x.Applications).HasForeignKey(x => x.CandidateProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Resume).WithMany().HasForeignKey(x => x.ResumeId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSelectionProcess(ModelBuilder builder)
    {
        builder.Entity<SelectionStage>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SelectionProcess>(entity =>
        {
            entity.HasIndex(x => x.JobApplicationId).IsUnique();
            entity.HasOne(x => x.JobApplication).WithOne(x => x.SelectionProcess).HasForeignKey<SelectionProcess>(x => x.JobApplicationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CurrentStage).WithMany(x => x.CurrentProcesses).HasForeignKey(x => x.CurrentStageId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StageMovementHistory>(entity =>
        {
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.ResultingStatus).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(x => x.SelectionProcess).WithMany(x => x.Movements).HasForeignKey(x => x.SelectionProcessId);
            entity.HasOne(x => x.FromStage).WithMany(x => x.FromMovements).HasForeignKey(x => x.FromStageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToStage).WithMany(x => x.ToMovements).HasForeignKey(x => x.ToStageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CandidateEvaluation>(entity =>
        {
            entity.Property(x => x.Comments).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.SelectionProcess).WithMany(x => x.Evaluations).HasForeignKey(x => x.SelectionProcessId);
            entity.HasOne(x => x.EvaluatorUser).WithMany().HasForeignKey(x => x.EvaluatorUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Feedback>(entity =>
        {
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.HasOne(x => x.SelectionProcess).WithMany(x => x.Feedbacks).HasForeignKey(x => x.SelectionProcessId);
            entity.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSupportTables(ModelBuilder builder)
    {
        builder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Title).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditLog>(entity =>
        {
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
        builder.Entity<Company>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CompanyAddress>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CompanyUser>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Skill>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<JobPosting>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<JobSkill>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CandidateProfile>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Resume>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Education>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<WorkExperience>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ResumeSkill>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CandidateDocument>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<JobApplication>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<SelectionProcess>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<SelectionStage>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<StageMovementHistory>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CandidateEvaluation>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Feedback>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AuditLog>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<JobApproval>().HasQueryFilter(x => !x.IsDeleted);
    }

    private void ApplyAuditDates()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
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
