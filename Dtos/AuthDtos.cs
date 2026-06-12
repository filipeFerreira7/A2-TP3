using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Dtos;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Cpf,
    string? Phone,
    string? AreaAtuacao,
    UserPermission PrimaryPermission = UserPermission.Candidate);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    CurrentUserResponse User);

public record CurrentUserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserPermission PrimaryPermission,
    IReadOnlyList<string> Roles,
    Guid? CompanyId);
