using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Dtos;

public record PerfilResponse(
    Guid Id,
    string FullName,
    string Cpf,
    DateOnly BirthDate,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? PortfolioUrl,
    string? AreaAtuacao,
    string? FotoPerfil,
    string? Summary,
    IReadOnlyList<EducacaoResponse> Educations,
    IReadOnlyList<ExperienciaResponse> WorkExperiences,
    IReadOnlyList<HabilidadeResponse> Skills);

public record EducacaoResponse(
    Guid Id,
    string Institution,
    string Course,
    string Degree,
    DateOnly StartDate,
    DateOnly? EndDate);

public record ExperienciaResponse(
    Guid Id,
    string CompanyName,
    string Position,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrentJob);

public record HabilidadeResponse(
    Guid Id,
    string Name,
    int ProficiencyLevel);

public record EducacaoRequest(
    string Institution,
    string Course,
    string Degree,
    DateOnly StartDate,
    DateOnly? EndDate);

public record ExperienciaRequest(
    string CompanyName,
    string Position,
    string Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrentJob);

public record HabilidadeRequest(
    Guid SkillId,
    int ProficiencyLevel);
