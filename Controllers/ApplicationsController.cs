using System.Security.Claims;
using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Route("api/candidaturas")]
public class ApplicationsController(JobConnectDbContext context, IWebHostEnvironment env, ILogger<ApplicationsController> logger) : ControllerBase
{
    [HttpPost("{jobId:guid}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply(Guid jobId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(item => item.Resume)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (profile is null)
        {
            return BadRequest(new { error = "Cadastre um perfil de candidato antes de se candidatar." });
        }

        if (profile.Resume is null)
        {
            return BadRequest(new { error = "Cadastre um curriculo antes de se candidatar." });
        }

        var job = await context.Vagas.FirstOrDefaultAsync(item =>
            item.Id == jobId &&
            item.Status == JobStatus.Published &&
            item.ClosingDate >= DateTime.UtcNow);

        if (job is null)
        {
            return BadRequest(new { error = "Vaga indisponivel para candidatura." });
        }

        var duplicated = await context.Candidaturas.AnyAsync(item =>
            item.JobPostingId == jobId &&
            item.CandidateProfileId == profile.Id);

        if (duplicated)
        {
            return Conflict(new { error = "Voce ja se candidatou a esta vaga." });
        }

        var initialStage = await context.EtapasSelecao
            .OrderBy(stage => stage.Order)
            .FirstOrDefaultAsync(stage => stage.CompanyId == job.CompanyId && stage.IsDefaultInitialStage)
            ?? CreateInitialStage(job.CompanyId);

        var application = new Candidatura
        {
            JobPostingId = job.Id,
            CandidateProfileId = profile.Id,
            ResumeId = profile.Resume.Id,
            Status = ApplicationStatus.Received
        };

        context.Candidaturas.Add(application);
        context.ProcessosSeletivos.Add(new ProcessoSeletivo
        {
            JobApplication = application,
            CurrentStage = initialStage
        });

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Mine), new { id = application.Id }, new { application.Id, application.Status });
    }

    [HttpPost("aplicar-com-perfil")]
    [Authorize(Roles = "Candidate")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ApplyWithProfile(
        [FromForm] Guid jobId,
        [FromForm] string fullName,
        [FromForm] string cpf,
        [FromForm] string? birthDate,
        [FromForm] string? phoneNumber,
        [FromForm] string? linkedInUrl,
        [FromForm] string? portfolioUrl,
        [FromForm] string? summary,
        [FromForm] string? availabilityPreference,
        [FromForm] string? salaryExpectation,
        [FromForm] string? experienceNotes,
        IFormFile? resumeFile)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var job = await context.Vagas.AsNoTracking().FirstOrDefaultAsync(item =>
            item.Id == jobId &&
            item.Status == JobStatus.Published &&
            item.ClosingDate >= DateTime.UtcNow);

        if (job is null)
            return BadRequest(new { error = "Vaga indisponivel para candidatura." });

        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        Curriculo resume;
        var uploadedFilePath = (string?)null;

        if (profile is null)
        {
            resume = new Curriculo
            {
                Summary = summary ?? string.Empty,
                IsPrimary = true
            };

            profile = new PerfilCandidato
            {
                UserId = userId,
                FullName = fullName,
                Cpf = cpf.Trim(),
                PhoneNumber = phoneNumber,
                LinkedInUrl = linkedInUrl,
                PortfolioUrl = portfolioUrl,
                Resume = resume
            };
            if (DateOnly.TryParse(birthDate ?? "", out var bd)) profile.BirthDate = bd;
            context.PerfisCandidatos.Add(profile);
        }
        else
        {
            profile.FullName = fullName;
            profile.Cpf = cpf.Trim();
            if (DateOnly.TryParse(birthDate, out var bd)) profile.BirthDate = bd;
            profile.PhoneNumber = phoneNumber;
            profile.LinkedInUrl = linkedInUrl;
            profile.PortfolioUrl = portfolioUrl;

            if (profile.Resume is null)
            {
                resume = new Curriculo
                {
                    CandidateProfile = profile,
                    Summary = summary ?? string.Empty,
                    IsPrimary = true
                };
                profile.Resume = resume;
                context.Curriculos.Add(resume);
            }
            else
            {
                resume = profile.Resume;
                resume.Summary = summary ?? string.Empty;
            }
        }

        if (resumeFile is not null && resumeFile.Length > 0)
        {
            if (!resumeFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Apenas arquivos PDF sao aceitos." });

            if (resumeFile.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "O arquivo deve ter no maximo 5MB." });

            var uploadsDir = Path.Combine(env.ContentRootPath, "uploads", "resumes");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid():N}.pdf";
            uploadedFilePath = Path.Combine(uploadsDir, fileName);

            await using var stream = new FileStream(uploadedFilePath, FileMode.Create);
            await resumeFile.CopyToAsync(stream);

            context.DocumentosCandidatos.Add(new DocumentoCandidato
            {
                ResumeId = resume.Id,
                FileName = resumeFile.FileName,
                ContentType = resumeFile.ContentType,
                StoragePath = fileName,
                SizeInBytes = resumeFile.Length,
                Type = DocumentType.ResumePdf
            });
        }

        var duplicated = await context.Candidaturas.AnyAsync(item =>
            item.JobPostingId == jobId &&
            item.CandidateProfileId == profile.Id);

        if (duplicated)
            return Conflict(new { error = "Voce ja se candidatou a esta vaga." });

        var initialStage = await context.EtapasSelecao
            .OrderBy(stage => stage.Order)
            .FirstOrDefaultAsync(stage => stage.CompanyId == job.CompanyId && stage.IsDefaultInitialStage)
            ?? CreateInitialStage(job.CompanyId);

        var application = new Candidatura
        {
            JobPostingId = job.Id,
            CandidateProfileId = profile.Id,
            ResumeId = resume.Id,
            Status = ApplicationStatus.Received,
            AvailabilityPreference = availabilityPreference,
            SalaryExpectation = salaryExpectation,
            ExperienceNotes = experienceNotes
        };

        context.Candidaturas.Add(application);
        context.ProcessosSeletivos.Add(new ProcessoSeletivo
        {
            JobApplication = application,
            CurrentStage = initialStage
        });

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException concurrencyEx)
        {
            var failedEntry = concurrencyEx.Entries.FirstOrDefault();
            var failedEntity = failedEntry?.Metadata.ClrType.Name ?? "desconhecida";
            var failedState = failedEntry?.State.ToString() ?? "?";
            var failedId = failedEntry?.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue;

            logger.LogError(
                "Concorrencia: entidade {Entity} (Id={Id}, State={State}) — 0 linhas afetadas no UPDATE.",
                failedEntity, failedId, failedState);

            if (uploadedFilePath is not null)
            {
                try { System.IO.File.Delete(uploadedFilePath); }
                catch { logger.LogWarning("Nao foi possivel limpar arquivo orfao: {Path}", uploadedFilePath); }
            }

            return Conflict(new
            {
                error = "Falha de concorrencia. Os dados foram modificados por outra requisicao. Tente novamente.",
                entity = failedEntity,
                entityId = failedId?.ToString()
            });
        }
        catch (DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? "(sem detalhe interno)";
            logger.LogError(ex, "Falha ao salvar candidatura (DbUpdateException). Inner: {Inner}", innerMessage);

            if (uploadedFilePath is not null)
            {
                try { System.IO.File.Delete(uploadedFilePath); }
                catch { logger.LogWarning("Nao foi possivel limpar arquivo orfao: {Path}", uploadedFilePath); }
            }

            return Conflict(new
            {
                error = "Falha ao salvar candidatura. Erro de banco de dados.",
                detail = innerMessage
            });
        }

        return Ok(new { application.Id, Message = "Candidatura registrada com sucesso!" });
    }

    [HttpGet("minhas")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> Mine()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var applications = await context.Candidaturas
            .AsNoTracking()
            .Include(item => item.CandidateProfile)
            .Include(item => item.JobPosting).ThenInclude(job => job.Company)
            .Include(item => item.SelectionProcess).ThenInclude(sp => sp!.CurrentStage)
            .Include(item => item.SelectionProcess).ThenInclude(sp => sp!.Feedbacks)
            .Where(item => item.CandidateProfile != null && item.CandidateProfile.UserId == userId)
            .OrderByDescending(item => item.AppliedAt)
            .ToListAsync();

        var result = new List<ApplicationResponse>();
        foreach (var app in applications)
        {
            var companyId = app.JobPosting.CompanyId;
            var allStages = await context.EtapasSelecao
                .AsNoTracking()
                .Where(s => s.CompanyId == companyId)
                .OrderBy(s => s.Order)
                .ToListAsync();

            if (allStages.Count == 0)
            {
                allStages =
                [
                    new EtapaSelecao { CompanyId = companyId, Name = "Inscricao Recebida", Order = 1, IsDefaultInitialStage = true },
                    new EtapaSelecao { CompanyId = companyId, Name = "Triagem", Order = 2 },
                    new EtapaSelecao { CompanyId = companyId, Name = "Entrevista Tecnica", Order = 3 },
                    new EtapaSelecao { CompanyId = companyId, Name = "Feedback Final", Order = 4 }
                ];
            }

            var process = app.SelectionProcess;
            var currentStage = process?.CurrentStage;
            var isFinished = process?.IsFinished ?? false;

            var stages = allStages.Select(s => new ProcessStageDto(
                s.Name,
                s.Order,
                currentStage?.Id == s.Id && !isFinished,
                currentStage is not null && (s.Order < currentStage.Order || (s.Order == currentStage.Order && isFinished))
            )).ToList();

            var feedback = process?.Feedbacks
                ?.OrderByDescending(f => f.CreatedAt)
                ?.Select(f => f.Message)
                ?.FirstOrDefault();

            result.Add(new ApplicationResponse(
                app.Id,
                app.JobPosting.Title,
                app.JobPosting.Company.TradeName,
                app.Status,
                app.AppliedAt,
                currentStage?.Name,
                stages,
                isFinished,
                feedback));
        }

        return Ok(result);
    }

    [HttpGet("vaga/{jobId:guid}")]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<ActionResult<IReadOnlyList<ApplicantResponse>>> ByJob(Guid jobId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var job = await context.Vagas.Include(j => j.Company).FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null) return NotFound();

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(u =>
                u.CompanyId == job.CompanyId && u.UserId == userId);
            if (!linked) return Forbid();
        }

        var applicants = await context.Candidaturas
            .AsNoTracking()
            .Include(a => a.CandidateProfile)
            .Include(a => a.JobPosting)
            .Include(a => a.Resume).ThenInclude(r => r!.Documents)
            .Include(a => a.Resume).ThenInclude(r => r!.Educations)
            .Include(a => a.Resume).ThenInclude(r => r!.WorkExperiences)
            .Include(a => a.Resume).ThenInclude(r => r!.Skills).ThenInclude(s => s.Skill)
            .Include(a => a.SelectionProcess).ThenInclude(sp => sp!.CurrentStage)
            .Where(a => a.JobPostingId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        var result = applicants.Select(a => new ApplicantResponse(
            a.Id,
            a.JobPosting.Id,
            a.JobPosting.Title,
            a.CandidateProfile?.Id,
            a.CandidateProfile?.FullName ?? "N/A",
            a.CandidateProfile?.Cpf ?? "N/A",
            a.CandidateProfile?.PhoneNumber,
            a.CandidateProfile?.LinkedInUrl,
            a.CandidateProfile?.PortfolioUrl,
            a.CandidateProfile?.AreaAtuacao,
            a.CandidateProfile?.FotoPerfil,
            a.Resume?.Summary ?? "",
            a.AvailabilityPreference,
            a.SalaryExpectation,
            a.ExperienceNotes,
            a.Status.ToString(),
            a.SelectionProcess?.CurrentStage?.Name,
            a.AppliedAt,
            a.Resume?.Documents
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefault()?.FileName))
            .ToList();

        return Ok(applicants);
    }

    [HttpPatch("{appId:guid}/status")]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<IActionResult> UpdateStatus(Guid appId, [FromBody] UpdateStatusRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var application = await context.Candidaturas
            .Include(a => a.JobPosting)
            .Include(a => a.SelectionProcess)
            .FirstOrDefaultAsync(a => a.Id == appId);

        if (application is null)
            return NotFound(new { error = "Candidatura nao encontrada." });

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(u =>
                u.CompanyId == application.JobPosting.CompanyId && u.UserId == userId);
            if (!linked) return Forbid();
        }

        if (!Enum.TryParse<ApplicationStatus>(request.Status, ignoreCase: true, out var newStatus))
            return BadRequest(new { error = "Status invalido." });

        application.Status = newStatus;

        var process = application.SelectionProcess;
        if (process is not null)
        {
            if (newStatus is ApplicationStatus.Rejected or ApplicationStatus.Withdrawn)
            {
                process.IsFinished = true;
                if (!string.IsNullOrWhiteSpace(request.Feedback))
                {
                    context.Feedbacks.Add(new Feedback
                    {
                        SelectionProcessId = process.Id,
                        CreatedByUserId = userId,
                        Message = request.Feedback.Trim(),
                        IsAutomatic = false,
                        SentAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                var allStages = await context.EtapasSelecao
                    .Where(s => s.CompanyId == application.JobPosting.CompanyId)
                    .OrderBy(s => s.Order)
                    .ToListAsync();

                var currentStage = process.CurrentStage;
                var currentOrder = currentStage?.Order ?? 0;

                EtapaSelecao? nextStage = null;
                if (newStatus == ApplicationStatus.InProgress)
                {
                    nextStage = allStages.FirstOrDefault(s => s.Order > currentOrder);
                }
                else if (newStatus == ApplicationStatus.Approved)
                {
                    nextStage = allStages.LastOrDefault();
                    process.IsFinished = true;
                }

                if (nextStage is not null && nextStage.Id != currentStage?.Id)
                {
                    process.CurrentStage = nextStage;

                    context.HistoricosMovimentosEtapas.Add(new HistoricoMovimentoEtapa
                    {
                        SelectionProcess = process,
                        FromStageId = currentStage?.Id,
                        ToStageId = nextStage.Id,
                        ChangedByUserId = userId
                    });
                }
            }
        }

        await context.SaveChangesAsync();

        return Ok(new { application.Id, application.Status });
    }

    [HttpGet("{id:guid}/processo")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<ProcessResponse>> GetProcess(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var application = await context.Candidaturas
            .AsNoTracking()
            .Include(a => a.CandidateProfile)
            .Include(a => a.JobPosting).ThenInclude(j => j.Company)
            .Include(a => a.SelectionProcess).ThenInclude(sp => sp!.CurrentStage)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
            return NotFound(new { error = "Candidatura nao encontrada." });

        if (application.CandidateProfile is null || application.CandidateProfile.UserId != userId)
            return Forbid();

        var companyId = application.JobPosting.CompanyId;

        var allStages = await context.EtapasSelecao
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .OrderBy(s => s.Order)
            .ToListAsync();

        if (allStages.Count == 0)
        {
            allStages =
            [
                new EtapaSelecao { CompanyId = companyId, Name = "Inscricao Recebida", Order = 1, IsDefaultInitialStage = true },
                new EtapaSelecao { CompanyId = companyId, Name = "Triagem", Order = 2 },
                new EtapaSelecao { CompanyId = companyId, Name = "Entrevista Tecnica", Order = 3 },
                new EtapaSelecao { CompanyId = companyId, Name = "Feedback Final", Order = 4 }
            ];
        }

        var currentStage = application.SelectionProcess?.CurrentStage;
        var isFinished = application.SelectionProcess?.IsFinished ?? false;

        var stages = allStages.Select(s => new ProcessStageDto(
            s.Name,
            s.Order,
            currentStage?.Id == s.Id && !isFinished,
            currentStage is not null && (s.Order < currentStage.Order || (s.Order == currentStage.Order && isFinished))
        )).ToList();

        var requiredSkills = await context.VagasHabilidades
            .AsNoTracking()
            .Include(vh => vh.Skill)
            .Where(vh => vh.JobPostingId == application.JobPostingId && vh.RequirementType == SkillRequirementType.Required)
            .Select(vh => vh.Skill.Name)
            .ToListAsync();

        return Ok(new ProcessResponse(
            application.Id,
            application.JobPosting.Title,
            application.JobPosting.Company.TradeName,
            application.Status.ToString(),
            currentStage?.Name,
            currentStage?.Order,
            isFinished,
            stages,
            application.JobPosting.WorkModel.ToString(),
            application.JobPosting.Level.ToString(),
            application.JobPosting.Description,
            requiredSkills
        ));
    }

    [HttpGet("{applicationId:guid}/curriculo")]
    [Authorize(Roles = "Candidate,Recruiter,Manager,Administrator")]
    public async Task<IActionResult> DownloadResume(Guid applicationId)
    {
        var application = await context.Candidaturas
            .Include(a => a.Resume).ThenInclude(r => r!.Documents)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application?.Resume is null || !application.Resume.Documents.Any())
            return NotFound(new { error = "Nenhum curriculo encontrado." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isCandidate = application.CandidateProfile?.UserId == userId;
        var isCompanyUser = false;

        if (!isCandidate)
        {
            var job = await context.Vagas.FindAsync(application.JobPostingId);
            if (job is not null)
            {
                var linked = await context.UsuariosEmpresa.AnyAsync(u =>
                    u.CompanyId == job.CompanyId && u.UserId == userId);
                if (linked || User.IsInRole("Administrator"))
                    isCompanyUser = true;
            }
        }

        if (!isCandidate && !isCompanyUser)
            return Forbid();

        var doc = application.Resume.Documents.OrderByDescending(d => d.CreatedAt).First();
        var filePath = Path.Combine(env.ContentRootPath, "uploads", "resumes", doc.StoragePath);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "Arquivo nao encontrado." });

        return PhysicalFile(filePath, "application/pdf", doc.FileName);
    }

    private EtapaSelecao CreateInitialStage(Guid companyId)
    {
        var stage = new EtapaSelecao
        {
            CompanyId = companyId,
            Name = "Inscricao Realizada",
            Order = 1,
            IsDefaultInitialStage = true
        };
        context.EtapasSelecao.Add(stage);
        return stage;
    }
}
