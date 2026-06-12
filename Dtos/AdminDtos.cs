using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Dtos;

public record AdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserPermission PrimaryPermission,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CompanyId);

public record AdminCompanyResponse(
    Guid Id,
    string LegalName,
    string TradeName,
    string Cnpj,
    string Email,
    bool IsActive,
    int JobCount,
    DateTime CreatedAt);

public record UpdateUserRoleRequest(string Role, Guid? CompanyId);

public record UpdateCompanyStatusRequest(bool IsActive);

public record UpdateCompanyRequest(
    string LegalName,
    string TradeName,
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

public record AdminReportResponse(
    int TotalUsers,
    int TotalCompanies,
    int TotalJobs,
    int TotalApplications,
    int TotalHires,
    int PendingApprovalJobs,
    int RejectedJobs,
    IReadOnlyList<JobStatsDto> JobsByStatus);

public record JobStatsDto(string Status, int Count);
