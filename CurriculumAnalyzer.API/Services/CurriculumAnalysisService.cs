using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Data.Entities;
using CurriculumAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Services;

public class CurriculumAnalysisService(
    IGrokAiService grokService,
    IFileProcessingService fileService,
    AppDbContext dbContext) : ICurriculumAnalysisService
{
    public async Task<string> ProcessUploadAsync(
        Stream fileStream,
        string fileName,
        string fileExtension,
        long fileSize,
        UserContextModel userContext,
        string userId)
    {
        var extractedText = await fileService.ExtractTextAsync(fileStream, fileExtension);

        if (string.IsNullOrWhiteSpace(extractedText))
            throw new Exceptions.ValidationException("Não foi possível extrair texto do arquivo.");

        var contentHash = ComputeHash(extractedText, userContext);

        var existing = await dbContext.Curriculums
            .Include(c => c.Analyses)
            .FirstOrDefaultAsync(c => c.ContentHash == contentHash && c.UserId == userId);

        if (existing?.Analyses.Count > 0)
            return existing.Analyses.First().Id;

        var curriculum = new CurriculumEntity
        {
            FileName = fileName,
            FileType = fileExtension,
            FileSize = fileSize,
            FirebaseUrl = string.Empty,
            RawText = extractedText,
            ExperienceLevel = userContext.ExperienceLevel,
            Specialization = userContext.Specialization,
            MarketObjective = userContext.MarketObjective,
            TargetSalary = userContext.TargetSalary,
            CurrentLocation = userContext.CurrentLocation,
            ContentHash = contentHash,
            UserId = userId
        };

        dbContext.Curriculums.Add(curriculum);
        await dbContext.SaveChangesAsync();

        var analysis = await grokService.AnalyzeCurriculumAsync(extractedText, userContext);

        var entity = new AnalysisEntity
        {
            CurriculumId = curriculum.Id,
            OverallScore = analysis.OverallScore,
            ScoreExplanation = analysis.ScoreExplanation,
            SectionsJson = JsonSerializer.Serialize(analysis.Sections),
            StrengthsJson = JsonSerializer.Serialize(analysis.Strengths),
            WeaknessesJson = JsonSerializer.Serialize(analysis.Weaknesses),
            OpportunitiesJson = JsonSerializer.Serialize(analysis.Opportunities),
            ActionPlanJson = JsonSerializer.Serialize(analysis.ActionPlan),
            JobRecommendationsJson = JsonSerializer.Serialize(analysis.JobRecommendations),
            EstimatedMinSalary = analysis.EstimatedSalaryRange.Min,
            EstimatedMaxSalary = analysis.EstimatedSalaryRange.Max,
            RawGrokResponse = analysis.RawResponse
        };

        dbContext.Analyses.Add(entity);
        await dbContext.SaveChangesAsync();

        return entity.Id;
    }

    private static string ComputeHash(string text, UserContextModel ctx)
    {
        var input = $"{text}|{ctx.ExperienceLevel}|{ctx.Specialization}|{ctx.MarketObjective}|{ctx.CurrentLocation}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
