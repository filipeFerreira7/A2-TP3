using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace a2_tp3_job_connect.Services;

public class TokenService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : ITokenService
{
    public async Task<string> GenerateJwtTokenAsync(ApplicationUser user, IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("Permission", user.PrimaryPermission.ToString())
        };

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.PrimaryPermission.ToString()));
        }
        else
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireHours = int.TryParse(configuration["Jwt:ExpireHours"], out var hours) ? hours : 8;

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expireHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
