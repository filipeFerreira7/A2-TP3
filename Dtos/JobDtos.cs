using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Dtos;

public record PublicCompanyResponse(
    Guid Id,
    string TradeName,
    string Email,
    string? LinkedInUrl,
    string? City,
    string? State);

public record CompanyDetailResponse(
    Guid Id,
    string LegalName,
    string TradeName,
    string Cnpj,
    string Email,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? Description,
    bool IsActive,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Complement,
    string? District,
    string? City,
    string? State);

public record PublicJobResponse(
    Guid Id,
    string Title,
    string Company,
    string Description,
    decimal? MinimumSalary,
    decimal? MaximumSalary,
    WorkModel WorkModel,
    JobLevel Level,
    int OpenPositions,
    DateTime? PublishedAt,
    DateTime ClosingDate,
    string? Tags,
    string? Benefits,
    string? Location,
    string? CompanyDescription,
    string? Responsibilities,
    string? Requirements,
    string? Schedule,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> DifferentialSkills);

public record CreateJobRequest(
    Guid CompanyId,
    string Title,
    string Description,
    decimal? MinimumSalary,
    decimal? MaximumSalary,
    WorkModel WorkModel,
    JobLevel Level,
    int OpenPositions,
    DateTime ClosingDate,
    string? Tags,
    string? Benefits,
    string? Location,
    string? CompanyDescription,
    string? Responsibilities,
    string? Requirements,
    string? Schedule,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> DifferentialSkills);

public record UpdateJobRequest(
    string Title,
    string Description,
    decimal? MinimumSalary,
    decimal? MaximumSalary,
    WorkModel WorkModel,
    JobLevel Level,
    int OpenPositions,
    DateTime ClosingDate,
    string? Tags,
    string? Benefits,
    string? Location,
    string? CompanyDescription,
    string? Responsibilities,
    string? Requirements,
    string? Schedule,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> DifferentialSkills);

public record EditJobResponse(
    Guid Id,
    string Title,
    string Description,
    decimal? MinimumSalary,
    decimal? MaximumSalary,
    WorkModel WorkModel,
    JobLevel Level,
    int OpenPositions,
    DateTime ClosingDate,
    string? Tags,
    string? Benefits,
    string? Location,
    string? CompanyDescription,
    string? Responsibilities,
    string? Requirements,
    string? Schedule,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<string> DifferentialSkills);

public record DashboardResponse(
    int PublishedJobs,
    int PendingJobs,
    int Applications,
    int Companies,
    IReadOnlyList<string> RecentNotifications);

