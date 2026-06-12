using System.Security.Claims;
using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Authorize]
[Route("api/vagas")]
public class JobsController(JobConnectDbContext context) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<IActionResult> Create(CreateJobRequest request)
    {
        var userId = GetUserId();
        var userIsAdmin = User.IsInRole("Administrator");

        var companyExists = await context.Empresas.AnyAsync(company => company.Id == request.CompanyId && company.IsActive);
        if (!companyExists)
        {
            return BadRequest("Empresa informada nao existe ou esta inativa.");
        }

        if (!userIsAdmin)
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(item =>
                item.CompanyId == request.CompanyId && item.UserId == userId);
            if (!linked)
            {
                return Forbid();
            }
        }

        if (request.OpenPositions <= 0 || request.ClosingDate <= DateTime.UtcNow)
        {
            return BadRequest("Informe quantidade de vagas e data de encerramento futuras.");
        }

        var job = new Vaga
        {
            CompanyId = request.CompanyId,
            CreatedByUserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            MinimumSalary = request.MinimumSalary,
            MaximumSalary = request.MaximumSalary,
            WorkModel = request.WorkModel,
            Level = request.Level,
            OpenPositions = request.OpenPositions,
            ClosingDate = request.ClosingDate,
            Tags = request.Tags,
            Benefits = request.Benefits,
            Location = request.Location,
            CompanyDescription = request.CompanyDescription,
            Responsibilities = request.Responsibilities,
            Requirements = request.Requirements,
            Schedule = request.Schedule,
            Status = JobStatus.PendingApproval
        };

        AddSkills(job, request.RequiredSkills, SkillRequirementType.Required);
        AddSkills(job, request.DifferentialSkills, SkillRequirementType.Differential);

        context.Vagas.Add(job);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMine), new { id = job.Id }, new { job.Id, job.Status });
    }

    [HttpGet("minhas")]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();
        IQueryable<Vaga> query = context.Vagas.AsNoTracking().Include(job => job.Company);

        if (!User.IsInRole("Administrator"))
        {
            var companyIds = await context.UsuariosEmpresa
                .Where(item => item.UserId == userId)
                .Select(item => item.CompanyId)
                .ToListAsync();

            query = query.Where(job => companyIds.Contains(job.CompanyId));
        }

        var jobs = await query
            .OrderByDescending(job => job.CreatedAt)
            .Select(job => new
            {
                job.Id,
                job.Title,
                Company = job.Company.TradeName,
                job.Status,
                job.OpenPositions,
                Applications = job.Applications.Count,
                Approved = job.Applications.Count(a => a.Status == ApplicationStatus.Approved),
                Rejected = job.Applications.Count(a => a.Status == ApplicationStatus.Rejected),
                RejectionReason = job.Approvals
                    .Where(a => !a.Approved)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.Notes)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetUserId();
        var job = await context.Vagas.Include(item => item.Company).FirstOrDefaultAsync(item => item.Id == id);
        if (job is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(item =>
                item.CompanyId == job.CompanyId && item.UserId == userId);
            if (!linked)
            {
                return Forbid();
            }
        }

        if (job.Status != JobStatus.PendingApproval)
        {
            return BadRequest("Vaga nao esta pendente de aprovacao.");
        }

        job.Status = JobStatus.Published;
        job.PublishedAt = DateTime.UtcNow;
        context.AprovacoesVagas.Add(new AprovacaoVaga
        {
            JobPostingId = job.Id,
            ApprovedByUserId = userId,
            Approved = true,
            Notes = "Aprovada pelo gestor."
        });

        await context.SaveChangesAsync();
        return Ok(new { job.Id, job.Status, job.PublishedAt });
    }

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = "Manager,Administrator")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { error = "Informe o motivo da rejeicao." });

        var userId = GetUserId();
        var job = await context.Vagas.Include(item => item.Company).FirstOrDefaultAsync(item => item.Id == id);
        if (job is null)
            return NotFound();

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(item =>
                item.CompanyId == job.CompanyId && item.UserId == userId);
            if (!linked)
                return Forbid();
        }

        if (job.Status != JobStatus.PendingApproval)
            return BadRequest(new { error = "Vaga nao esta pendente de aprovacao." });

        job.Status = JobStatus.Rejected;
        job.UpdatedAt = DateTime.UtcNow;
        context.AprovacoesVagas.Add(new AprovacaoVaga
        {
            JobPostingId = job.Id,
            ApprovedByUserId = userId,
            Approved = false,
            Notes = request.Reason.Trim()
        });

        await context.SaveChangesAsync();
        return Ok(new { job.Id, job.Status });
    }

    [HttpGet("{id:guid}/editar")]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<IActionResult> GetForEdit(Guid id)
    {
        var userId = GetUserId();
        var job = await context.Vagas
            .AsNoTracking()
            .Include(j => j.Company)
            .Include(j => j.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);

        if (job is null) return NotFound();

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(u =>
                u.CompanyId == job.CompanyId && u.UserId == userId);
            if (!linked) return Forbid();
        }

        return Ok(new EditJobResponse(
            job.Id, job.Title, job.Description,
            job.MinimumSalary, job.MaximumSalary,
            job.WorkModel, job.Level,
            job.OpenPositions, job.ClosingDate,
            job.Tags, job.Benefits, job.Location,
            job.CompanyDescription, job.Responsibilities, job.Requirements, job.Schedule,
            job.Skills.Where(s => s.RequirementType == SkillRequirementType.Required).Select(s => s.Skill.Name).ToList(),
            job.Skills.Where(s => s.RequirementType == SkillRequirementType.Differential).Select(s => s.Skill.Name).ToList()));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<IActionResult> Update(Guid id, UpdateJobRequest request)
    {
        var userId = GetUserId();
        var job = await context.Vagas
            .Include(j => j.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);

        if (job is null) return NotFound();

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(u =>
                u.CompanyId == job.CompanyId && u.UserId == userId);
            if (!linked) return Forbid();
        }

        if (request.OpenPositions <= 0 || request.ClosingDate <= DateTime.UtcNow)
            return BadRequest("Informe quantidade de vagas e data de encerramento futuras.");

        job.Title = request.Title.Trim();
        job.Description = request.Description.Trim();
        job.MinimumSalary = request.MinimumSalary;
        job.MaximumSalary = request.MaximumSalary;
        job.WorkModel = request.WorkModel;
        job.Level = request.Level;
        job.OpenPositions = request.OpenPositions;
        job.ClosingDate = request.ClosingDate;
        job.Tags = request.Tags;
        job.Benefits = request.Benefits;
        job.Location = request.Location;
        job.CompanyDescription = request.CompanyDescription;
        job.Responsibilities = request.Responsibilities;
        job.Requirements = request.Requirements;
        job.Schedule = request.Schedule;
        job.UpdatedAt = DateTime.UtcNow;

        context.VagasHabilidades.RemoveRange(job.Skills);
        job.Skills.Clear();
        AddSkills(job, request.RequiredSkills, SkillRequirementType.Required);
        AddSkills(job, request.DifferentialSkills, SkillRequirementType.Differential);

        await context.SaveChangesAsync();
        return Ok(new { job.Id, job.Status, job.UpdatedAt });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Recruiter,Manager,Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var job = await context.Vagas.FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);
        if (job is null) return NotFound();

        if (!User.IsInRole("Administrator"))
        {
            var linked = await context.UsuariosEmpresa.AnyAsync(u =>
                u.CompanyId == job.CompanyId && u.UserId == userId);
            if (!linked) return Forbid();
        }

        job.IsDeleted = true;
        job.DeletedAt = DateTime.UtcNow;
        job.Status = JobStatus.Closed;
        job.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok(new { job.Id, Status = "Deleted" });
    }

    private void AddSkills(Vaga job, IEnumerable<string> skillNames, SkillRequirementType type)
    {
        foreach (var skillName in skillNames.Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalized = skillName.Trim();
            var skill = context.Habilidades.Local.FirstOrDefault(item => item.Name == normalized) ??
                context.Habilidades.FirstOrDefault(item => item.Name == normalized) ??
                new Habilidade { Name = normalized };

            job.Skills.Add(new VagaHabilidade
            {
                Skill = skill,
                RequirementType = type
            });
        }
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
