using System.Security.Claims;
using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using a2_tp3_job_connect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ITokenService tokenService,
    JobConnectDbContext context) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PrimaryPermission = request.PrimaryPermission,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(error => error.Description));
        }

        await AssignRoleAsync(user, request.PrimaryPermission.ToString());
        var token = await tokenService.GenerateJwtTokenAsync(user);
        return Ok(new AuthResponse(token, await ToResponseAsync(user)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            return Unauthorized(new { message = "Email ou senha invalidos." });
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Email ou senha invalidos." });
        }

        var token = await tokenService.GenerateJwtTokenAsync(user);
        return Ok(new AuthResponse(token, await ToResponseAsync(user)));
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? Unauthorized() : Ok(await ToResponseAsync(user));
    }

    private async Task AssignRoleAsync(ApplicationUser user, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
        await userManager.AddToRoleAsync(user, roleName);
    }

    private async Task<CurrentUserResponse> ToResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var companyId = await context.UsuariosEmpresa
            .Where(u => u.UserId == user.Id)
            .Select(u => (Guid?)u.CompanyId)
            .FirstOrDefaultAsync();
        return new CurrentUserResponse(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PrimaryPermission,
            roles.ToList(),
            companyId);
    }
}
