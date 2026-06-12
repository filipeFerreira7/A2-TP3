using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Dtos;

public record ApplyToJobRequest(Guid JobId);

public record ApplyWithProfileRequest(
    Guid JobId,
    string FullName,
    string Cpf,
    DateOnly? BirthDate,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? PortfolioUrl,
    string Summary);

public record ApplicationResponse(
    Guid Id,
    string JobTitle,
    string Company,
    ApplicationStatus Status,
    DateTime AppliedAt,
    string? CurrentStage,
    IReadOnlyList<ProcessStageDto>? Stages,
    bool IsFinished,
    string? FeedbackMessage);

public record ProcessStageDto(
    string Name,
    int Order,
    bool IsCurrent,
    bool IsCompleted
);

public record ProcessResponse(
    Guid ApplicationId,
    string JobTitle,
    string Company,
    string Status,
    string? CurrentStageName,
    int? CurrentStageOrder,
    bool IsFinished,
    IReadOnlyList<ProcessStageDto> Stages,
    string WorkModel,
    string Level,
    string Description,
    IReadOnlyList<string> RequiredSkills
);

public record UpdateStatusRequest(string Status, string? Feedback);

public record ApplicantResponse(
    Guid ApplicationId,
    Guid JobId,
    string JobTitle,
    Guid? CandidateProfileId,
    string CandidateName,
    string Cpf,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? PortfolioUrl,
    string? AreaAtuacao,
    string? FotoPerfil,
    string Summary,
    string? AvailabilityPreference,
    string? SalaryExpectation,
    string? ExperienceNotes,
    string Status,
    string? CurrentStage,
    DateTime AppliedAt,
    string? ResumeFileName);
