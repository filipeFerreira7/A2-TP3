using System.Security.Claims;
using a2_tp3_job_connect.Data;
using a2_tp3_job_connect.Dtos;
using a2_tp3_job_connect.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace a2_tp3_job_connect.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize(Roles = "Candidate")]
public class PerfilController(JobConnectDbContext context, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PerfilResponse>> Get()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await context.PerfisCandidatos
            .AsNoTracking()
            .Include(p => p.Resume!).ThenInclude(r => r.Educations)
            .Include(p => p.Resume!).ThenInclude(r => r.WorkExperiences)
            .Include(p => p.Resume!).ThenInclude(r => r.Skills).ThenInclude(s => s.Skill)
            .Include(p => p.Resume!).ThenInclude(r => r.Documents)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            return Ok(null);

        return Ok(ToResponse(profile));
    }

    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PerfilResponse>> Update(
        [FromForm] string fullName,
        [FromForm] string cpf,
        [FromForm] string? birthDate,
        [FromForm] string? phoneNumber,
        [FromForm] string? linkedInUrl,
        [FromForm] string? portfolioUrl,
        [FromForm] string? areaAtuacao,
        [FromForm] string? summary,
        IFormFile? fotoPerfil,
        IFormFile? resumeFile)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume!).ThenInclude(r => r.Educations)
            .Include(p => p.Resume!).ThenInclude(r => r.WorkExperiences)
            .Include(p => p.Resume!).ThenInclude(r => r.Skills).ThenInclude(s => s.Skill)
            .Include(p => p.Resume!).ThenInclude(r => r.Documents)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
        {
            var resume = new Curriculo
            {
                Summary = summary ?? string.Empty,
                IsPrimary = true
            };

            profile = new PerfilCandidato
            {
                UserId = userId,
                FullName = fullName,
                Cpf = cpf.Trim(),
                PhoneNumber = phoneNumber,
                LinkedInUrl = linkedInUrl,
                PortfolioUrl = portfolioUrl,
                AreaAtuacao = areaAtuacao,
                Resume = resume
            };

            if (DateOnly.TryParse(birthDate ?? "", out var bd))
                profile.BirthDate = bd;

            context.PerfisCandidatos.Add(profile);
        }
        else
        {
            profile.FullName = fullName;
            profile.Cpf = cpf.Trim();
            if (DateOnly.TryParse(birthDate, out var bd))
                profile.BirthDate = bd;
            profile.PhoneNumber = phoneNumber;
            profile.LinkedInUrl = linkedInUrl;
            profile.PortfolioUrl = portfolioUrl;
            profile.AreaAtuacao = areaAtuacao;

            if (profile.Resume is null)
            {
                var resume = new Curriculo
                {
                    CandidateProfile = profile,
                    Summary = summary ?? string.Empty,
                    IsPrimary = true
                };
                profile.Resume = resume;
                context.Curriculos.Add(resume);
            }
            else
            {
                profile.Resume.Summary = summary ?? string.Empty;
            }
        }

        if (fotoPerfil is not null && fotoPerfil.Length > 0)
        {
            if (!fotoPerfil.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Apenas arquivos de imagem sao aceitos para foto de perfil." });

            if (fotoPerfil.Length > 2 * 1024 * 1024)
                return BadRequest(new { error = "A foto deve ter no maximo 2MB." });

            var fotosDir = Path.Combine(env.ContentRootPath, "uploads", "fotos");
            Directory.CreateDirectory(fotosDir);
            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(fotoPerfil.FileName)}";
            var filePath = Path.Combine(fotosDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await fotoPerfil.CopyToAsync(stream);

            if (profile.FotoPerfil is not null)
            {
                var oldPath = Path.Combine(fotosDir, profile.FotoPerfil);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            profile.FotoPerfil = fileName;
        }

        if (resumeFile is not null && resumeFile.Length > 0)
        {
            if (!resumeFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Apenas arquivos PDF sao aceitos para curriculo." });

            if (resumeFile.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "O arquivo deve ter no maximo 5MB." });

            var resumesDir = Path.Combine(env.ContentRootPath, "uploads", "resumes");
            Directory.CreateDirectory(resumesDir);
            var fileName = $"{Guid.NewGuid():N}.pdf";
            var filePath = Path.Combine(resumesDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await resumeFile.CopyToAsync(stream);

            context.DocumentosCandidatos.Add(new DocumentoCandidato
            {
                ResumeId = profile.Resume!.Id,
                FileName = resumeFile.FileName,
                ContentType = resumeFile.ContentType,
                StoragePath = fileName,
                SizeInBytes = resumeFile.Length,
                Type = DocumentType.ResumePdf
            });
        }

        await context.SaveChangesAsync();

        return Ok(ToResponse(profile));
    }

    [HttpPost("foto")]
    public async Task<IActionResult> UploadFoto(IFormFile file)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            return NotFound(new { error = "Perfil nao encontrado. Crie seu perfil primeiro." });

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Apenas arquivos de imagem sao aceitos." });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { error = "A foto deve ter no maximo 2MB." });

        var fotosDir = Path.Combine(env.ContentRootPath, "uploads", "fotos");
        Directory.CreateDirectory(fotosDir);
        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(fotosDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        if (profile.FotoPerfil is not null)
        {
            var oldPath = Path.Combine(fotosDir, profile.FotoPerfil);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        profile.FotoPerfil = fileName;
        await context.SaveChangesAsync();

        return Ok(new { fotoPerfil = fileName });
    }

    [HttpPost("educacao")]
    public async Task<IActionResult> AddEducacao([FromBody] EducacaoRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Crie seu perfil primeiro." });

        var formacao = new Formacao
        {
            ResumeId = profile.Resume.Id,
            Institution = request.Institution,
            Course = request.Course,
            Degree = request.Degree,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        context.Formacoes.Add(formacao);
        await context.SaveChangesAsync();
        return Ok(new { formacao.Id });
    }

    [HttpPost("experiencia")]
    public async Task<IActionResult> AddExperiencia([FromBody] ExperienciaRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Crie seu perfil primeiro." });

        var exp = new ExperienciaProfissional
        {
            ResumeId = profile.Resume.Id,
            CompanyName = request.CompanyName,
            Position = request.Position,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCurrentJob = request.IsCurrentJob
        };
        context.ExperienciasProfissionais.Add(exp);
        await context.SaveChangesAsync();
        return Ok(new { exp.Id });
    }

    [HttpDelete("educacao/{id}")]
    public async Task<IActionResult> DeleteEducacao(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Perfil nao encontrado." });

        var formacao = await context.Formacoes.FindAsync(id);
        if (formacao is null || formacao.ResumeId != profile.Resume.Id)
            return NotFound(new { error = "Formacao nao encontrada." });

        context.Formacoes.Remove(formacao);
        await context.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    [HttpDelete("experiencia/{id}")]
    public async Task<IActionResult> DeleteExperiencia(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Perfil nao encontrado." });

        var exp = await context.ExperienciasProfissionais.FindAsync(id);
        if (exp is null || exp.ResumeId != profile.Resume.Id)
            return NotFound(new { error = "Experiencia nao encontrada." });

        context.ExperienciasProfissionais.Remove(exp);
        await context.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    [HttpPost("habilidades")]
    public async Task<IActionResult> AddHabilidade([FromBody] HabilidadeRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Crie seu perfil primeiro." });

        var skill = await context.Habilidades.FindAsync(request.SkillId);
        if (skill is null)
            return BadRequest(new { error = "Habilidade nao encontrada." });

        var existing = await context.CurriculosHabilidades
            .AnyAsync(ch => ch.ResumeId == profile.Resume.Id && ch.SkillId == request.SkillId);
        if (existing)
            return Conflict(new { error = "Habilidade ja adicionada ao curriculo." });

        var ch = new CurriculoHabilidade
        {
            ResumeId = profile.Resume.Id,
            SkillId = request.SkillId,
            ProficiencyLevel = request.ProficiencyLevel
        };
        context.CurriculosHabilidades.Add(ch);
        await context.SaveChangesAsync();
        return Ok(new { ch.Id });
    }

    [HttpGet("resume/{id}")]
    public async Task<IActionResult> DownloadResume(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume!).ThenInclude(r => r.Documents)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Perfil nao encontrado." });

        var doc = profile.Resume.Documents.FirstOrDefault(d => d.Id == id && d.Type == DocumentType.ResumePdf);
        if (doc is null)
            return NotFound(new { error = "Curriculo nao encontrado." });

        var resumesDir = Path.Combine(env.ContentRootPath, "uploads", "resumes");
        var filePath = Path.Combine(resumesDir, doc.StoragePath);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "Arquivo nao encontrado no servidor." });

        return PhysicalFile(filePath, doc.ContentType, doc.FileName);
    }

    [HttpDelete("resume/{id}")]
    public async Task<IActionResult> DeleteResume(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await context.PerfisCandidatos
            .Include(p => p.Resume!).ThenInclude(r => r.Documents)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile?.Resume is null)
            return BadRequest(new { error = "Perfil nao encontrado." });

        var doc = profile.Resume.Documents.FirstOrDefault(d => d.Id == id && d.Type == DocumentType.ResumePdf);
        if (doc is null)
            return NotFound(new { error = "Curriculo nao encontrado." });

        var resumesDir = Path.Combine(env.ContentRootPath, "uploads", "resumes");
        var filePath = Path.Combine(resumesDir, doc.StoragePath);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        context.DocumentosCandidatos.Remove(doc);
        await context.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    private static PerfilResponse ToResponse(PerfilCandidato profile)
    {
        var resume = profile.Resume;
        return new PerfilResponse(
            profile.Id,
            profile.FullName,
            profile.Cpf,
            profile.BirthDate,
            profile.PhoneNumber,
            profile.LinkedInUrl,
            profile.PortfolioUrl,
            profile.AreaAtuacao,
            profile.FotoPerfil,
            resume?.Summary,
            resume?.Educations.Select(e => new EducacaoResponse(
                e.Id, e.Institution, e.Course, e.Degree, e.StartDate, e.EndDate)
            ).ToList() ?? [],
            resume?.WorkExperiences.Select(e => new ExperienciaResponse(
                e.Id, e.CompanyName, e.Position, e.Description, e.StartDate, e.EndDate, e.IsCurrentJob)
            ).ToList() ?? [],
            resume?.Skills.Select(s => new HabilidadeResponse(
                s.Skill.Id, s.Skill.Name, s.ProficiencyLevel)
            ).ToList() ?? [],
            resume?.Documents
                .Where(d => d.Type == DocumentType.ResumePdf)
                .Select(d => new DocumentoResponse(d.Id, d.FileName, d.StoragePath, d.SizeInBytes))
                .FirstOrDefault());
    }
}
