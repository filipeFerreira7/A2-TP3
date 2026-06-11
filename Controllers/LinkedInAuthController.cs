using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using a2_tp3_job_connect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Route("api/auth/linkedin")]
public class LinkedInAuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ITokenService tokenService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, byte> States = new();

    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        var clientId = configuration["LinkedIn:ClientId"];
        if (string.IsNullOrEmpty(clientId))
            return BadRequest(new { error = "LinkedIn OAuth nao configurado. Defina LinkedIn:ClientId em appsettings.json." });

        var redirectUri = configuration["LinkedIn:RedirectUri"];
        var encodedReturn = Uri.EscapeDataString(returnUrl ?? "/dashboard");
        var state = $"{encodedReturn}|{GenerateState()}";
        States.TryAdd(state, 0);

        var authorizationUrl = $"https://www.linkedin.com/oauth/v2/authorization" +
            $"?response_type=code" +
            $"&client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
            $"&scope={Uri.EscapeDataString("openid profile email")}" +
            $"&state={Uri.EscapeDataString(state)}";

        return Redirect(authorizationUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery] string? error_description)
    {
        var frontendUrl = configuration["LinkedIn:FrontendUrl"] ?? "http://localhost:5173";

        if (!string.IsNullOrEmpty(error))
        {
            var desc = Uri.EscapeDataString(error_description ?? error);
            return Redirect($"{frontendUrl}/login?error={desc}");
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Parametros invalidos na autenticacao.")}");
        }

        var pipeIndex = state.IndexOf('|');
        var returnUrl = pipeIndex >= 0 ? Uri.UnescapeDataString(state[..pipeIndex]) : "/dashboard";
        var statePart = pipeIndex >= 0 ? state[(pipeIndex + 1)..] : state;

        if (!States.TryRemove(state, out _))
        {
            return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Parametros invalidos na autenticacao.")}");
        }

        try
        {
            var accessToken = await ExchangeCodeForTokenAsync(code);

            if (string.IsNullOrEmpty(accessToken))
                return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Falha ao obter token do LinkedIn.")}");

            var userInfo = await GetUserInfoAsync(accessToken);

            if (string.IsNullOrEmpty(userInfo.Email))
                return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Nao foi possivel obter seu email do LinkedIn.")}");

            var user = await FindOrCreateUserAsync(userInfo);

            await userManager.SetAuthenticationTokenAsync(user, "LinkedIn", "access_token", accessToken);

            var additionalClaims = new List<Claim> { new("LoginProvider", "LinkedIn") };
            var jwt = await tokenService.GenerateJwtTokenAsync(user, additionalClaims);

            return Redirect($"{frontendUrl}/login?token={Uri.EscapeDataString(jwt)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<LinkedInAuthController>>();
            logger.LogError(ex, "Erro no callback do LinkedIn OAuth");
            return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Erro interno na autenticacao com LinkedIn.")}");
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Unauthorized();

        var linkedInToken = await userManager.GetAuthenticationTokenAsync(user, "LinkedIn", "access_token");
        if (string.IsNullOrEmpty(linkedInToken))
            return Unauthorized(new { message = "Usuário não conectado ao LinkedIn" });

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new("Bearer", linkedInToken);

            var response = await client.GetAsync("https://api.linkedin.com/v2/userinfo");
            if (!response.IsSuccessStatusCode)
                return BadRequest(new { message = "Erro ao buscar dados do LinkedIn" });

            var profile = await response.Content.ReadFromJsonAsync<LinkedInProfileResponse>();

            return Ok(new
            {
                fullName = profile?.Name,
                email = profile?.Email,
                emailVerified = profile?.EmailVerified,
                picture = profile?.Picture,
                linkedInId = profile?.Sub
            });
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<LinkedInAuthController>>();
            logger.LogError(ex, "Erro ao buscar perfil do LinkedIn");
            return BadRequest(new { message = "Erro ao buscar dados do LinkedIn" });
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(string code)
    {
        var client = httpClientFactory.CreateClient();
        var redirectUri = configuration["LinkedIn:RedirectUri"];
        var clientId = configuration["LinkedIn:ClientId"];
        var clientSecret = configuration["LinkedIn:ClientSecret"];

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri!,
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!
        };

        var response = await client.PostAsync(
            "https://www.linkedin.com/oauth/v2/accessToken",
            new FormUrlEncodedContent(parameters));

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<LinkedInAuthController>>();
            logger.LogWarning("LinkedIn token exchange failed: {StatusCode} {Body}", response.StatusCode, errorBody);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("access_token", out var token) ? token.GetString() : null;
    }

    private async Task<LinkedInUserInfo> GetUserInfoAsync(string accessToken)
    {
        var client = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new LinkedInUserInfo
        {
            Sub = root.TryGetProperty("sub", out var sub) ? sub.GetString() : null,
            Name = root.TryGetProperty("name", out var name) ? name.GetString() : null,
            GivenName = root.TryGetProperty("given_name", out var given) ? given.GetString() : null,
            FamilyName = root.TryGetProperty("family_name", out var family) ? family.GetString() : null,
            Picture = root.TryGetProperty("picture", out var pic) ? pic.GetString() : null,
            Email = root.TryGetProperty("email", out var email) ? email.GetString() : null,
            EmailVerified = root.TryGetProperty("email_verified", out var verified) && verified.GetBoolean()
        };
    }

    private async Task<ApplicationUser> FindOrCreateUserAsync(LinkedInUserInfo info)
    {
        var user = !string.IsNullOrEmpty(info.Email)
            ? await userManager.FindByEmailAsync(info.Email)
            : null;

        if (user is not null)
            return user;

        var email = info.Email ?? $"{info.Sub}@linkedin.placeholder";
        var fullName = info.Name ?? info.GivenName ?? info.FamilyName ?? "LinkedIn User";

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PrimaryPermission = UserPermission.Candidate,
            EmailConfirmed = info.EmailVerified
        };

        var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var result = await userManager.CreateAsync(user, randomPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Falha ao criar usuario do LinkedIn: {errors}");
        }

        var roleName = UserPermission.Candidate.ToString();
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));

        await userManager.AddToRoleAsync(user, roleName);

        return user;
    }

    private static string GenerateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var sb = new StringBuilder(64);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private record LinkedInUserInfo
    {
        public string? Sub { get; init; }
        public string? Name { get; init; }
        public string? GivenName { get; init; }
        public string? FamilyName { get; init; }
        public string? Picture { get; init; }
        public string? Email { get; init; }
        public bool EmailVerified { get; init; }
    }

    public record LinkedInProfileResponse
    {
        public string? Sub { get; init; }
        public string? Name { get; init; }
        public string? GivenName { get; init; }
        public string? FamilyName { get; init; }
        public string? Picture { get; init; }
        public string? Email { get; init; }
        public bool EmailVerified { get; init; }
    }
}
