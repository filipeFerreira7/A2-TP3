using System.Net.Http.Headers;
using a2_tp3_job_connect.Dtos;

namespace a2_tp3_job_connect.Services;

public interface ILinkedInService
{
    Task<LinkedInProfileResponse> InspectProfileAsync(string url, CancellationToken cancellationToken = default);
}

public class LinkedInService(HttpClient httpClient, IConfiguration configuration) : ILinkedInService
{
    public async Task<LinkedInProfileResponse> InspectProfileAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            !uri.Host.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase))
        {
            return new LinkedInProfileResponse(url, "Perfil invalido", false, "Informe uma URL publica do LinkedIn.");
        }

        var token = configuration["LinkedIn:AccessToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            return new LinkedInProfileResponse(
                uri.ToString(),
                BuildDisplayName(uri),
                false,
                "URL validada. Configure LinkedIn:AccessToken para consumir a API oficial autenticada.");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await httpClient.GetAsync("https://api.linkedin.com/v2/userinfo", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new LinkedInProfileResponse(
                uri.ToString(),
                BuildDisplayName(uri),
                true,
                "Token configurado, mas a API do LinkedIn recusou a consulta atual.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return new LinkedInProfileResponse(uri.ToString(), BuildDisplayName(uri), true, content);
    }

    private static string BuildDisplayName(Uri uri)
    {
        var segment = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault() ?? "linkedin";

        return segment.Replace('-', ' ').Replace('_', ' ');
    }
}

