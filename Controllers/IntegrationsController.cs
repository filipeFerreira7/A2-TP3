using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Services;
using Microsoft.AspNetCore.Mvc;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Route("api/integrations")]
public class IntegrationsController(
    IViaCepService viaCepService,
    ILinkedInService linkedInService) : ControllerBase
{
    [HttpGet("viacep/{cep}")]
    public async Task<ActionResult<ViaCepResponse>> ViaCep(string cep, CancellationToken cancellationToken)
    {
        var address = await viaCepService.SearchAsync(cep, cancellationToken);
        return address is null || address.Erro ? NotFound() : Ok(address);
    }

    [HttpPost("linkedin/profile")]
    public async Task<ActionResult<LinkedInProfileResponse>> LinkedInProfile(
        LinkedInProfileRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await linkedInService.InspectProfileAsync(request.Url, cancellationToken));
    }
}

