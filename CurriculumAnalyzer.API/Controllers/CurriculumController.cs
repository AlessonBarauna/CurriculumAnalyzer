using System.Text.Json;
using CurriculumAnalyzer.API.Exceptions;
using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/curriculum")]
public class CurriculumController(ICurriculumAnalysisService analysisService) : ControllerBase
{
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    ];

    [HttpPost("upload-and-analyze")]
    public async Task<IActionResult> UploadAndAnalyze(IFormFile file, [FromForm] string context)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Arquivo não fornecido." });

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Tipo de arquivo não suportado. Use PDF, DOCX ou TXT." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "Arquivo muito grande. Máximo 5MB." });

        UserContextModel? userContext;
        try
        {
            userContext = JsonSerializer.Deserialize<UserContextModel>(context,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return BadRequest(new { error = "Contexto inválido." });
        }

        if (userContext == null)
            return BadRequest(new { error = "Contexto não fornecido." });

        var extension = Path.GetExtension(file.FileName).TrimStart('.');

        using var stream = file.OpenReadStream();
        var analysisId = await analysisService.ProcessUploadAsync(
            stream, file.FileName, extension, file.Length, userContext);

        return Ok(new { analysisId });
    }
}
