using System.Text.Json;
using System.Text.Json.Serialization;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Services;

public class AnalysisService(AppDbContext dbContext) : IAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<IEnumerable<object>> GetHistoryAsync(string userId)
    {
        return await dbContext.Analyses
            .Include(a => a.Curriculum)
            .Where(a => a.Curriculum.UserId == userId)
            .OrderByDescending(a => a.AnalysisDate)
            .Select(a => new
            {
                a.Id,
                a.OverallScore,
                a.AnalysisDate,
                a.Curriculum.FileName,
                a.Curriculum.ExperienceLevel,
                a.Curriculum.Specialization,
                a.Curriculum.MarketObjective,
                SalaryMin = a.EstimatedMinSalary,
                SalaryMax = a.EstimatedMaxSalary
            })
            .ToListAsync<object>();
    }

    public async Task<AnalysisResponseDto?> GetByIdAsync(string id, string userId)
    {
        var analysis = await dbContext.Analyses
            .Include(a => a.Curriculum)
            .FirstOrDefaultAsync(a => a.Id == id && a.Curriculum.UserId == userId);

        return analysis is null ? null : MapToDto(analysis);
    }

    public async Task<(AnalysisResponseDto Before, AnalysisResponseDto After)?> CompareAsync(string id1, string id2, string userId)
    {
        var analyses = await dbContext.Analyses
            .Include(a => a.Curriculum)
            .Where(a => (a.Id == id1 || a.Id == id2) && a.Curriculum.UserId == userId)
            .OrderBy(a => a.AnalysisDate)
            .ToListAsync();

        if (analyses.Count < 2) return null;

        return (MapToDto(analyses[0]), MapToDto(analyses[1]));
    }

    private static AnalysisResponseDto MapToDto(Data.Entities.AnalysisEntity a) => new()
    {
        Id = a.Id,
        CurriculumId = a.CurriculumId,
        AnalysisDate = a.AnalysisDate,
        OverallScore = a.OverallScore,
        ScoreExplanation = a.ScoreExplanation,
        EstimatedSalaryRange = new SalaryRangeDto { Min = a.EstimatedMinSalary, Max = a.EstimatedMaxSalary, Currency = "BRL" },
        Sections = TryDeserialize<Dictionary<string, SectionScoreDto>>(a.SectionsJson) ?? new(),
        Strengths = TryDeserialize<List<FeedbackDto>>(a.StrengthsJson) ?? new(),
        Weaknesses = TryDeserialize<List<FeedbackDto>>(a.WeaknessesJson) ?? new(),
        Opportunities = TryDeserialize<List<OpportunityDto>>(a.OpportunitiesJson) ?? new(),
        ActionPlan = TryDeserialize<List<ActionItemDto>>(a.ActionPlanJson) ?? new(),
        JobRecommendations = TryDeserialize<List<JobRecommendationDto>>(a.JobRecommendationsJson) ?? new()
    };

    private static T? TryDeserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
        catch { return default; }
    }
}
