namespace a2_tp3_job_connect.Dtos;

public record ViaCepResponse(
    string Cep,
    string Logradouro,
    string Bairro,
    string Localidade,
    string Uf,
    bool Erro);

public record LinkedInProfileRequest(string Url);

public record LinkedInProfileResponse(
    string Url,
    string DisplayName,
    bool UsesOfficialApi,
    string Message);

