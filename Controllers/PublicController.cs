using System.Security.Claims;
using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Route("api/vagas")]
public class PublicController(JobConnectDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicJobResponse>>> List([FromQuery] string? search, [FromQuery] int? limit)
    {
        var query = context.Vagas
            .AsNoTracking()
            .Include(job => job.Company)
            .Include(job => job.Skills).ThenInclude(jobSkill => jobSkill.Skill)
            .Where(job => job.Status == JobStatus.Published && job.ClosingDate >= DateTime.UtcNow);

        if (User.Identity?.IsAuthenticated == true &&
            (User.IsInRole("Recruiter") || User.IsInRole("Manager")) &&
            !User.IsInRole("Administrator"))
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var companyIds = await context.UsuariosEmpresa
                .Where(u => u.UserId == userId)
                .Select(u => u.CompanyId)
                .ToListAsync();
            query = query.Where(job => companyIds.Contains(job.CompanyId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(job =>
                job.Title.Contains(search) ||
                job.Description.Contains(search) ||
                (job.Tags != null && job.Tags.Contains(search)));
        }

        query = query.OrderByDescending(job => job.PublishedAt).Take(limit ?? 30);

        var jobs = await query.ToListAsync();

        return Ok(jobs.Select(ToPublicJob).ToList());
    }

    [HttpGet("stats")]
    public async Task<ActionResult> Stats()
    {
        var totalVagas = await context.Vagas.CountAsync(job => job.Status == JobStatus.Published && job.ClosingDate >= DateTime.UtcNow);
        var totalEmpresas = await context.Empresas.CountAsync(company => company.IsActive);
        var totalCandidatos = await context.PerfisCandidatos.CountAsync();
        var totalContratacoes = await context.ProcessosSeletivos.CountAsync(sp => sp.IsFinished);

        return Ok(new { totalVagas, totalEmpresas, totalCandidatos, totalContratacoes });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicJobResponse>> GetById(Guid id)
    {
        var job = await context.Vagas
            .AsNoTracking()
            .Include(item => item.Company)
            .Include(item => item.Skills).ThenInclude(item => item.Skill)
            .FirstOrDefaultAsync(item => item.Id == id && item.Status == JobStatus.Published);

        return job is null ? NotFound() : Ok(ToPublicJob(job));
    }

    [HttpGet("~/api/empresas")]
    public async Task<ActionResult<IReadOnlyList<PublicCompanyResponse>>> Companies()
    {
        var companies = await context.Empresas
            .AsNoTracking()
            .Include(company => company.Address)
            .Where(company => company.IsActive)
            .OrderBy(company => company.TradeName)
            .Select(company => new PublicCompanyResponse(
                company.Id,
                company.TradeName,
                company.Email,
                company.LinkedInUrl,
                company.Address != null ? company.Address.City : null,
                company.Address != null ? company.Address.State : null))
            .ToListAsync();

        return Ok(companies);
    }

    [HttpGet("~/api/empresas/{id:guid}")]
    public async Task<ActionResult<CompanyDetailResponse>> CompanyDetail(Guid id)
    {
        var company = await context.Empresas
            .AsNoTracking()
            .Include(company => company.Address)
            .FirstOrDefaultAsync(company => company.Id == id && company.IsActive);

        return company is null
            ? NotFound()
            : Ok(new CompanyDetailResponse(
                company.Id,
                company.LegalName,
                company.TradeName,
                company.Cnpj,
                company.Email,
                company.PhoneNumber,
                company.LinkedInUrl,
                company.Description,
                company.IsActive,
                company.Address?.ZipCode,
                company.Address?.Street,
                company.Address?.Number,
                company.Address?.Complement,
                company.Address?.District,
                company.Address?.City,
                company.Address?.State));
    }

    [HttpGet("~/api/habilidades")]
    public async Task<ActionResult<IReadOnlyList<object>>> Skills()
    {
        var skills = await context.Habilidades
            .AsNoTracking()
            .OrderBy(skill => skill.Name)
            .Select(skill => new { skill.Id, skill.Name })
            .ToListAsync();

        return Ok(skills);
    }

    private static PublicJobResponse ToPublicJob(Vaga job)
    {
        return new PublicJobResponse(
            job.Id,
            job.Title,
            job.Company.TradeName,
            job.Description,
            job.MinimumSalary,
            job.MaximumSalary,
            job.WorkModel,
            job.Level,
            job.OpenPositions,
            job.PublishedAt,
            job.ClosingDate,
            job.Tags,
            job.Benefits,
            job.Location,
            job.CompanyDescription,
            job.Responsibilities,
            job.Requirements,
            job.Schedule,
            job.Skills.Where(skill => skill.RequirementType == SkillRequirementType.Required).Select(skill => skill.Skill.Name).ToList(),
            job.Skills.Where(skill => skill.RequirementType == SkillRequirementType.Differential).Select(skill => skill.Skill.Name).ToList());
    }
}
