using System.Text.Json;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Data.Entities;
using CurriculumAnalyzer.API.Models;
using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public class CurriculumAnalysisService
{
    private readonly GrokAiService _grokService;
    private readonly AppDbContext _dbContext;

    public CurriculumAnalysisService(GrokAiService grokService, AppDbContext dbContext)
    {
        _grokService = grokService;
        _dbContext = dbContext;
    }

    public async Task<AnalysisResponseDto> AnalyzeCurriculumAsync(
        string curriculumId,
        string curriculumText,
        UserContextModel userContext)
    {
        var grokAnalysis = await _grokService.AnalyzeCurriculumAsync(curriculumText, userContext);

        var entity = new AnalysisEntity
        {
            CurriculumId = curriculumId,
            OverallScore = grokAnalysis.OverallScore,
            ScoreExplanation = grokAnalysis.ScoreExplanation,
            SectionsJson = JsonSerializer.Serialize(grokAnalysis.Sections),
            StrengthsJson = JsonSerializer.Serialize(grokAnalysis.Strengths),
            WeaknessesJson = JsonSerializer.Serialize(grokAnalysis.Weaknesses),
            OpportunitiesJson = JsonSerializer.Serialize(grokAnalysis.Opportunities),
            ActionPlanJson = JsonSerializer.Serialize(grokAnalysis.ActionPlan),
            JobRecommendationsJson = JsonSerializer.Serialize(grokAnalysis.JobRecommendations),
            EstimatedMinSalary = grokAnalysis.EstimatedSalaryRange.Min,
            EstimatedMaxSalary = grokAnalysis.EstimatedSalaryRange.Max,
            RawGrokResponse = grokAnalysis.RawResponse
        };

        _dbContext.Analyses.Add(entity);
        await _dbContext.SaveChangesAsync();

        grokAnalysis.Id = entity.Id;
        grokAnalysis.CurriculumId = curriculumId;
        grokAnalysis.AnalysisDate = entity.AnalysisDate;
        return grokAnalysis;
    }
}
