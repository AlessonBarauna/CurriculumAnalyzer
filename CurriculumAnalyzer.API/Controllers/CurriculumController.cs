using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Data.Entities;
using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/curriculum")]
public class CurriculumController : ControllerBase
{
    private readonly ICurriculumAnalysisService _analysisService;
    private readonly IFileProcessingService _fileService;
    private readonly AppDbContext _dbContext;

    public CurriculumController(
        ICurriculumAnalysisService analysisService,
        IFileProcessingService fileService,
        AppDbContext dbContext)
    {
        _analysisService = analysisService;
        _fileService = fileService;
        _dbContext = dbContext;
    }

    [HttpPost("upload-and-analyze")]
    public async Task<IActionResult> UploadAndAnalyze(IFormFile file, [FromForm] string context)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Arquivo não fornecido." });

        var allowedTypes = new[]
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/plain"
        };

        if (!allowedTypes.Contains(file.ContentType))
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

        string extractedText;
        using (var stream = file.OpenReadStream())
        {
            extractedText = await _fileService.ExtractTextAsync(stream, extension);
        }

        if (string.IsNullOrWhiteSpace(extractedText))
            return BadRequest(new { error = "Não foi possível extrair texto do arquivo." });

        var contentHash = ComputeHash(extractedText, userContext);

        var existing = await _dbContext.Curriculums
            .Include(c => c.Analyses)
            .FirstOrDefaultAsync(c => c.ContentHash == contentHash);

        if (existing?.Analyses.Count > 0)
            return Ok(new { analysisId = existing.Analyses.First().Id });

        var curriculum = new CurriculumEntity
        {
            FileName = file.FileName,
            FileType = extension,
            FileSize = file.Length,
            FirebaseUrl = string.Empty,
            RawText = extractedText,
            ExperienceLevel = userContext.ExperienceLevel,
            Specialization = userContext.Specialization,
            MarketObjective = userContext.MarketObjective,
            TargetSalary = userContext.TargetSalary,
            CurrentLocation = userContext.CurrentLocation,
            ContentHash = contentHash
        };

        _dbContext.Curriculums.Add(curriculum);
        await _dbContext.SaveChangesAsync();

        var analysis = await _analysisService.AnalyzeCurriculumAsync(curriculum.Id, extractedText, userContext);

        return Ok(new { analysisId = analysis.Id });
    }

    private static string ComputeHash(string text, UserContextModel ctx)
    {
        var input = $"{text}|{ctx.ExperienceLevel}|{ctx.Specialization}|{ctx.MarketObjective}|{ctx.CurrentLocation}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
