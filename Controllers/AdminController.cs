using System.Security.Claims;
using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin")]
public class AdminController(
    JobConnectDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers()
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PrimaryPermission,
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync();

        var companyLinks = await context.UsuariosEmpresa
            .AsNoTracking()
            .Where(ue => ue.IsActive)
            .GroupBy(ue => ue.UserId)
            .Select(g => new { UserId = g.Key, CompanyId = g.First().CompanyId })
            .ToListAsync();

        var companyByUser = companyLinks.ToDictionary(c => c.UserId, c => (Guid?)c.CompanyId);

        var result = new List<AdminUserResponse>();
        foreach (var u in users)
        {
            var appUser = await userManager.FindByIdAsync(u.Id.ToString());
            var roles = appUser is not null ? await userManager.GetRolesAsync(appUser) : [];
            var cid = companyByUser.GetValueOrDefault(u.Id);
            result.Add(new AdminUserResponse(u.Id, u.FullName, u.Email ?? "", u.PrimaryPermission, roles.ToList(), u.IsActive, u.CreatedAt, cid));
        }

        return Ok(result);
    }

    [HttpPut("usuarios/{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new { error = "Usuario nao encontrado." });

        if (!Enum.TryParse<UserPermission>(request.Role, ignoreCase: true, out var newPermission))
            return BadRequest(new { error = "Role invalida." });

        if ((newPermission == UserPermission.Recruiter || newPermission == UserPermission.Manager) && request.CompanyId is null)
            return BadRequest(new { error = "Selecione uma empresa para vincular o usuario." });

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);

        var roleName = newPermission.ToString();
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        await userManager.AddToRoleAsync(user, roleName);

        user.PrimaryPermission = newPermission;

        var existingLinks = await context.UsuariosEmpresa.Where(ue => ue.UserId == id).ToListAsync();
        context.UsuariosEmpresa.RemoveRange(existingLinks);

        if (request.CompanyId.HasValue)
        {
            context.UsuariosEmpresa.Add(new UsuarioEmpresa
            {
                UserId = id,
                CompanyId = request.CompanyId.Value,
                Role = newPermission == UserPermission.Manager ? CompanyUserRole.Manager : CompanyUserRole.Recruiter,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();

        return Ok(new { message = "Permissoes atualizadas com sucesso." });
    }

    [HttpPut("usuarios/{id:guid}/status")]
    public async Task<IActionResult> ToggleUserStatus(Guid id, [FromBody] UpdateCompanyStatusRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return NotFound(new { error = "Usuario nao encontrado." });

        user.IsActive = request.IsActive;
        await context.SaveChangesAsync();

        return Ok(new { message = request.IsActive ? "Usuario ativado." : "Usuario desativado." });
    }

    [HttpGet("empresas")]
    public async Task<ActionResult<IReadOnlyList<AdminCompanyResponse>>> GetCompanies()
    {
        var companies = await context.Empresas
            .AsNoTracking()
            .OrderBy(c => c.TradeName)
            .Select(c => new AdminCompanyResponse(
                c.Id, c.LegalName, c.TradeName, c.Cnpj, c.Email, c.IsActive,
                c.Jobs.Count, c.CreatedAt))
            .ToListAsync();

        return Ok(companies);
    }

    [HttpPut("empresas/{id:guid}")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        var company = await context.Empresas.Include(e => e.Address).FirstOrDefaultAsync(c => c.Id == id);
        if (company is null)
            return NotFound(new { error = "Empresa nao encontrada." });

        company.LegalName = request.LegalName.Trim();
        company.TradeName = request.TradeName.Trim();
        company.Email = request.Email.Trim();
        company.PhoneNumber = request.PhoneNumber?.Trim();
        company.LinkedInUrl = request.LinkedInUrl?.Trim();
        company.Description = request.Description?.Trim();
        company.IsActive = request.IsActive;
        company.UpdatedAt = DateTime.UtcNow;

        if (company.Address is null && (request.ZipCode is not null || request.Street is not null))
        {
            company.Address = new EnderecoEmpresa();
        }

        if (company.Address is not null)
        {
            company.Address.ZipCode = request.ZipCode?.Trim();
            company.Address.Street = request.Street?.Trim();
            company.Address.Number = request.Number?.Trim();
            company.Address.Complement = request.Complement?.Trim();
            company.Address.District = request.District?.Trim();
            company.Address.City = request.City?.Trim();
            company.Address.State = request.State?.Trim();
        }

        await context.SaveChangesAsync();
        return Ok(new { message = "Empresa atualizada com sucesso." });
    }

    [HttpPut("empresas/{id:guid}/status")]
    public async Task<IActionResult> ToggleCompanyStatus(Guid id, [FromBody] UpdateCompanyStatusRequest request)
    {
        var company = await context.Empresas.FirstOrDefaultAsync(c => c.Id == id);
        if (company is null)
            return NotFound(new { error = "Empresa nao encontrada." });

        company.IsActive = request.IsActive;
        await context.SaveChangesAsync();

        return Ok(new { message = request.IsActive ? "Empresa ativada." : "Empresa desativada." });
    }

    [HttpDelete("empresas/{id:guid}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        var company = await context.Empresas.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (company is null)
            return NotFound(new { error = "Empresa nao encontrada." });

        company.IsDeleted = true;
        company.DeletedAt = DateTime.UtcNow;
        company.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Ok(new { message = "Empresa excluida com sucesso." });
    }

    [HttpGet("relatorios")]
    public async Task<ActionResult<AdminReportResponse>> GetReports()
    {
        var totalUsers = await context.Users.CountAsync();
        var totalCompanies = await context.Empresas.CountAsync();
        var totalJobs = await context.Vagas.CountAsync();
        var totalApplications = await context.Candidaturas.CountAsync();
        var totalHires = await context.ProcessosSeletivos.CountAsync(sp => sp.IsFinished);
        var pendingApprovalJobs = await context.Vagas.CountAsync(j => j.Status == JobStatus.PendingApproval);
        var rejectedJobs = await context.Vagas.CountAsync(j => j.Status == JobStatus.Rejected);

        var jobsByStatus = await context.Vagas
            .GroupBy(j => j.Status)
            .Select(g => new JobStatsDto(g.Key.ToString(), g.Count()))
            .ToListAsync();

        return Ok(new AdminReportResponse(
            totalUsers, totalCompanies, totalJobs, totalApplications,
            totalHires, pendingApprovalJobs, rejectedJobs, jobsByStatus));
    }
}
