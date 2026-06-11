using System.Security.Claims;
using a2_tp3_job_connect.Entities;

namespace a2_tp3_job_connect.Services;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(ApplicationUser user, IEnumerable<Claim>? additionalClaims = null);
}
