using System.Text.Json;
using a2_tp3_job_connect.Dtos;

namespace a2_tp3_job_connect.Services;

public interface IViaCepService
{
    Task<ViaCepResponse?> SearchAsync(string cep, CancellationToken cancellationToken = default);
}

public class ViaCepService(HttpClient httpClient) : IViaCepService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ViaCepResponse?> SearchAsync(string cep, CancellationToken cancellationToken = default)
    {
        var digits = new string(cep.Where(char.IsDigit).ToArray());
        if (digits.Length != 8)
        {
            return null;
        }

        using var response = await httpClient.GetAsync($"https://viacep.com.br/ws/{digits}/json/", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<ViaCepResponse>(stream, JsonOptions, cancellationToken);
    }
}

