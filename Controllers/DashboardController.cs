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
[Route("api/dashboard")]
public class DashboardController(JobConnectDbContext context) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<DashboardResponse>> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (User.IsInRole("Administrator"))
        {
            return Ok(await BuildAdminDashboard());
        }

        if (User.IsInRole("Recruiter") || User.IsInRole("Manager"))
        {
            return Ok(await BuildCompanyDashboard(userId));
        }

        return Ok(await BuildCandidateDashboard(userId));
    }

    private async Task<DashboardResponse> BuildAdminDashboard()
    {
        var publishedJobs = await context.Vagas.CountAsync(job => job.Status == JobStatus.Published);
        var pendingJobs = await context.Vagas.CountAsync(job => job.Status == JobStatus.PendingApproval);
        var applications = await context.Candidaturas.CountAsync();
        var companies = await context.Empresas.CountAsync(company => company.IsActive);
        var notifications = await context.Notificacoes
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .Take(5)
            .Select(item => item.Title)
            .ToListAsync();

        return new DashboardResponse(publishedJobs, pendingJobs, applications, companies, notifications);
    }

    private async Task<DashboardResponse> BuildCompanyDashboard(Guid userId)
    {
        var companyIds = await context.UsuariosEmpresa
            .Where(item => item.UserId == userId)
            .Select(item => item.CompanyId)
            .ToListAsync();

        var publishedJobs = await context.Vagas.CountAsync(job =>
            companyIds.Contains(job.CompanyId) && job.Status == JobStatus.Published);
        var pendingJobs = await context.Vagas.CountAsync(job =>
            companyIds.Contains(job.CompanyId) && job.Status == JobStatus.PendingApproval);
        var applications = await context.Candidaturas
            .CountAsync(app => app.JobPosting != null && companyIds.Contains(app.JobPosting.CompanyId));
        var companies = companyIds.Count;
        var notifications = await context.Notificacoes
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Take(5)
            .Select(item => item.Title)
            .ToListAsync();

        return new DashboardResponse(publishedJobs, pendingJobs, applications, companies, notifications);
    }

    private async Task<DashboardResponse> BuildCandidateDashboard(Guid userId)
    {
        var publishedJobs = await context.Vagas.CountAsync(job =>
            job.Status == JobStatus.Published && job.ClosingDate >= DateTime.UtcNow);
        var profile = await context.PerfisCandidatos
            .FirstOrDefaultAsync(item => item.UserId == userId);

        var applications = profile is not null
            ? await context.Candidaturas.CountAsync(item => item.CandidateProfileId == profile.Id)
            : 0;

        var companies = await context.Empresas.CountAsync(company => company.IsActive);
        var notifications = await context.Notificacoes
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Take(5)
            .Select(item => item.Title)
            .ToListAsync();

        return new DashboardResponse(publishedJobs, 0, applications, companies, notifications);
    }
}
