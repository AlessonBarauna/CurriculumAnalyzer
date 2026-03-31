using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using CurriculumAnalyzer.API.Exceptions;
using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/curriculum")]
[Authorize]
public class CurriculumController(ICurriculumAnalysisService analysisService) : ControllerBase
{
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    ];

    [HttpPost("upload-and-analyze")]
    public async Task<IActionResult> UploadAndAnalyze(IFormFile file, [FromForm] string? context)
    {
        if (string.IsNullOrWhiteSpace(context))
            return BadRequest(new { error = "Contexto não fornecido." });

        UserContextModel? userContext;
        try
        {
            userContext = JsonSerializer.Deserialize<UserContextModel>(context,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            return BadRequest(new { error = $"JSON inválido: {ex.Message}" });
        }

        if (userContext == null)
            return BadRequest(new { error = "Contexto desserializado como nulo." });

        // Validações customizadas
        if (string.IsNullOrEmpty(userContext.ExperienceLevel))
            return BadRequest(new { error = "ExperienceLevel é obrigatório." });

        var extension = Path.GetExtension(file.FileName).TrimStart('.');

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new ValidationException("Usuário não identificado.");

        using var stream = file.OpenReadStream();
        var analysisId = await analysisService.ProcessUploadAsync(
            stream, file.FileName, extension, file.Length, userContext, userId);

        return Ok(new { analysisId });
    }
}